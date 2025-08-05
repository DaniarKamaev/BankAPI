using BankAPI.Shared.Models;
using MediatR;

namespace BankAPI.Features.Accounts.Create;

public record CreateAccount(
    Guid OwnerId,
    AccountType Type,
    CurrencyType Currency,
    decimal Balance,
    decimal? InterestRate) : IRequest<CreateAccountResponse>;

