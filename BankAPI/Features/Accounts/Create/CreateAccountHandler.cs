using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;

namespace BankAPI.Features.Accounts.Create;

public class CreateAccountHandler(BankDbContext db) : IRequestHandler<CreateAccount, CreateAccountResponse>
{
    public async Task<CreateAccountResponse> Handle(CreateAccount request, CancellationToken cancellationToken)
    {
        var account = new Shared.Models.Account
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency,
            Balance = request.Balance,
            InterestRate = request.InterestRate,
            OpenDate = DateTime.Now,
            Transactions = new List<Transaction>()
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateAccountResponse(
            $"Счёт {account.Type} открыт",
            account.Id);
    }
}