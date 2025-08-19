using BankAPI.Shared;
using BankAPI.Shared.Models;
using BankAPI.Shared.Models.RabbitModel;
using MediatR;

namespace BankAPI.Features.Accounts.Create;

public class CreateAccountHandler(BankDbContext db, RabbitMqService rabbitMq) : IRequestHandler<CreateAccount, CreateAccountResponse>
{
    public async Task<CreateAccountResponse> Handle(CreateAccount request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Создание учетной записи для OwnerId: {request.OwnerId}");
        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency,
            Balance = request.Balance,
            InterestRate = request.InterestRate,
            OpenDate = DateTime.UtcNow,
            Transactions = new List<Transaction>()
        };

        await db.Accounts.AddAsync(account, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var @event = new AccountOpenedEvent(account.Id,
            account.OwnerId,
            account.Currency.ToString(),
            account.Type.ToString());

        rabbitMq.Publish(@event, "Учетная запись созданна", Guid.NewGuid());
        Console.WriteLine($"Учетная запись, созданная с использованием идентификатора: {account.Id}");

        return new CreateAccountResponse($"Счёт {account.Type} открыт", account.Id);
    }
}