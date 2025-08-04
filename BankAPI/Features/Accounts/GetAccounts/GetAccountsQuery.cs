using BankAPI.Shared.Models;
using MediatR;

namespace BankAPI.Features.Accounts.GetAccounts;

public record GetAccountsQuery(Guid OwnerId) : IRequest<IEnumerable<Account>>;
