using BankAPI.Shared;
using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;

namespace BankAPI.Features.Accounts.GetAccounts;

public class GetAccountsHandler(BankDbContext db) : IRequestHandler<GetAccountsQuery, IEnumerable<Account>>
{
    public Task<IEnumerable<Account>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = db.Accounts.Where(a => a.OwnerId == request.OwnerId).ToList();
        return Task.FromResult<IEnumerable<Account>>(accounts);
    }
}