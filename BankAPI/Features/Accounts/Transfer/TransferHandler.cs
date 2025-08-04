using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;

namespace BankAPI.Features.Accounts.Transfer;

public class TransferHandler(BankDbContext db) : IRequestHandler<TransferRequest, string>
{
    public async Task<string> Handle(TransferRequest request, CancellationToken cancellationToken)
    {
        var fromAccount = db.Accounts.FirstOrDefault(a => a.Id == request.FromAccountId);
        var toAccount = db.Accounts.FirstOrDefault(a => a.Id == request.ToAccountId);

        if (fromAccount == null || toAccount == null)
            throw new Exception("Один из счетов не найден");
        

        if (fromAccount.Balance < request.Amount)
            throw new Exception("Недостаточно средств");

        if (fromAccount.Currency != toAccount.Currency)
            throw new Exception("Разные валюты");



        //списание
        fromAccount.Balance -= request.Amount;
        fromAccount.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = fromAccount.Id,
            CounterpartyAccountId = toAccount.Id,
            Amount = request.Amount,
            Currency = fromAccount.Currency,
            Type = TransactionType.Debit,
            Description = $"Перевод на счёт {toAccount.Id}",
            DateTime = DateTime.Now
        });

        //зачисление
        toAccount.Balance += request.Amount;
        toAccount.Transactions.Add(new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = toAccount.Id,
            CounterpartyAccountId = fromAccount.Id,
            Amount = request.Amount,
            Currency = toAccount.Currency,
            Type = TransactionType.Credit,
            Description = $"Перевод со счёта {fromAccount.Id}",
            DateTime = DateTime.Now
        });

        await db.SaveChangesAsync(cancellationToken);
        return "Перевод выполнен успешно";
    }
}