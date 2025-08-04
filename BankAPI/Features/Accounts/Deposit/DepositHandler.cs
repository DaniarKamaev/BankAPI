using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;

namespace BankAPI.Features.Accounts.Deposit;

public class DepositHandler(BankDbContext db) : IRequestHandler<DepositRequest, string>
{
    public async Task<string> Handle(DepositRequest request, CancellationToken cancellationToken)
    {
        var account = db.Accounts.FirstOrDefault(a => a.Id == request.AccountId);
        if (account == null) throw new Exception("Account not found");

        account.Balance += request.Amount;
        account.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            Amount = request.Amount,
            Currency = account.Currency,
            Type = TransactionType.Credit,
            Description = "Пополнение наличными",
            DateTime = DateTime.Now
        });

        await db.SaveChangesAsync(cancellationToken);
        return $"Счёт пополнен на {request.Amount} {account.Currency}";
    }
}