using MediatR;

namespace BankAPI.Features.Accounts.Deposit;

public record DepositRequest(Guid AccountId, decimal Amount) : IRequest<DepositResponse>;