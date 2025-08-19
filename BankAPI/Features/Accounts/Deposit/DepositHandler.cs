using BankAPI.Shared;
using BankAPI.Shared.Models;
using BankAPI.Shared.Models.RabbitModel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Features.Accounts.Deposit;


public class DepositHandler(BankDbContext db, RabbitMqService rabbitMq) : IRequestHandler<DepositRequest, DepositResponse>
{
    public async Task<DepositResponse> Handle(DepositRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Учетная запись с идентификатором {request.AccountId} не найдена.");

            if (request.Amount <= 0)
                throw new InvalidOperationException("Сумма депозита должна быть положительной.");

            account.Balance += request.Amount;

            var deposit = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                CounterpartyAccountId = null, // Для депозита контрагент не требуется
                Amount = request.Amount,
                Currency = (CurrencyType)account.Currency, // Используем валюту счёта
                Type = TransactionType.Debit,
                Description = $"Deposit of {request.Amount} to Account {request.AccountId}",
                DateTime = DateTime.UtcNow
            };

            await db.Transactions.AddAsync(deposit, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var @event = new AccountOpenedEvent(
             account.Id,
             account.OwnerId,
             account.Currency.ToString(),
             account.Type.ToString());

            rabbitMq.Publish(@event, "Депозит упешно внесен", Guid.NewGuid());

            Console.WriteLine($"Deposited {request.Amount} to AccountId: {request.AccountId}");
            return new DepositResponse(
                "Депозит выполнен",
                account.Balance);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in DepositHandler: {ex.Message}");
            throw;
        }
    }
}