using BankAPI.Shared;
using BankAPI.Shared.Models;
using BankAPI.Shared.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BankAPI.Features.Accounts.Transfer;

public class TransferHandler : IRequestHandler<TransferRequest, TransferResponse>
{
    private readonly BankDbContext _db;
    private readonly RabbitMqService _rabbitMq;
    private readonly ILogger<TransferHandler> _logger;

    public TransferHandler(
        BankDbContext db,
        RabbitMqService rabbitMq,
        ILogger<TransferHandler> logger)
    {
        _db = db;
        _rabbitMq = rabbitMq;
        _logger = logger;
    }

    public async Task<TransferResponse> Handle(TransferRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var fromAccount = await _db.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.FromAccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Account {request.FromAccountId} not found");

            var toAccount = await _db.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.ToAccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Account {request.ToAccountId} not found");

            // Проверки
            if (fromAccount.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient funds");

            if (fromAccount.Currency != toAccount.Currency)
                throw new InvalidOperationException("Currency mismatch");

            if (fromAccount.IsLocked)
                throw new InvalidOperationException("Account is loked");

            // Выполняем перевод
            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            // Явно отмечаем изменения
            _db.Entry(fromAccount).State = EntityState.Modified;
            _db.Entry(toAccount).State = EntityState.Modified;

            // Создаем транзакции
            var withdrawal = new BankAPI.Shared.Models.Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.FromAccountId,
                CounterpartyAccountId = request.ToAccountId,
                Amount = -request.Amount,
                Currency = fromAccount.Currency,
                Type = TransactionType.Credit,
                Description = $"Transfer to {request.ToAccountId}",
                DateTime = DateTime.UtcNow
            };

            var deposit = new BankAPI.Shared.Models.Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.ToAccountId,
                CounterpartyAccountId = request.FromAccountId,
                Amount = request.Amount,
                Currency = toAccount.Currency,
                Type = TransactionType.Debit,
                Description = $"Transfer from {request.FromAccountId}",
                DateTime = DateTime.UtcNow
            };

            await _db.Transactions.AddAsync(withdrawal, cancellationToken);
            await _db.Transactions.AddAsync(deposit, cancellationToken);

            // Сохраняем Outbox сообщение
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = "TransferCompleted",
                EventData = JsonSerializer.Serialize(new
                {
                    TransactionId = withdrawal.Id,
                    FromAccountId = request.FromAccountId,
                    ToAccountId = request.ToAccountId,
                    Amount = request.Amount,
                    Currency = fromAccount.Currency.ToString(),
                    Timestamp = DateTime.UtcNow
                }),
                CreatedAt = DateTime.UtcNow
            };

            await _db.OutboxMessages.AddAsync(outboxMessage, cancellationToken);


            // Перед SaveChanges
            _logger.LogInformation("Saving {TransactionsCount} transactions and {OutboxCount} outbox messages",
                2, 1);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("SaveChanges successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveChanges failed");
                throw;
            }

            // Сохраняем все изменения
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new TransferResponse(
                "Transfer completed successfully",
                fromAccount.Balance,
                toAccount.Balance);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transfer failed");
            throw;
        }
    }

}