using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

            if (fromAccount.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient funds");

            if (fromAccount.Currency != toAccount.Currency)
                throw new InvalidOperationException("Currency mismatch");

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            var withdrawal = new Shared.Models.Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.FromAccountId,
                CounterpartyAccountId = request.ToAccountId,
                Amount = -request.Amount,
                Currency = (CurrencyType)fromAccount.Currency,
                Type = TransactionType.Credit,
                Description = $"Transfer to {request.ToAccountId}",
                DateTime = DateTime.UtcNow
            };

            var deposit = new Shared.Models.Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.ToAccountId,
                CounterpartyAccountId = request.FromAccountId,
                Amount = request.Amount,
                Currency = (CurrencyType)toAccount.Currency,
                Type = TransactionType.Debit,
                Description = $"Transfer from {request.FromAccountId}",
                DateTime = DateTime.UtcNow
            };

            await _db.Transactions.AddAsync(withdrawal, cancellationToken);
            await _db.Transactions.AddAsync(deposit, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            var transferEvent = new
            {
                TransactionId = withdrawal.Id,
                FromAccountId = request.FromAccountId,
                ToAccountId = request.ToAccountId,
                Amount = request.Amount,
                Currency = withdrawal.Currency.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _rabbitMq.Publish(
                message: transferEvent,
                routingKey: "Перевод успешно выполнен",
                correlationId: Guid.NewGuid());

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Transfer completed: {Amount} from {From} to {To}",
                request.Amount, request.FromAccountId, request.ToAccountId);

            return new TransferResponse(
                "Transfer completed successfully",
                fromAccount.Balance,
                toAccount.Balance);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Transfer failed from {From} to {To}",
                request.FromAccountId, request.ToAccountId);
            throw;
        }
    }
}