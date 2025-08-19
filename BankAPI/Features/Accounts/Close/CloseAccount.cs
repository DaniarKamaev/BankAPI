using MediatR;

namespace BankAPI.Features.Accounts.Close;

public record CloseAccount(Guid AccountId) : IRequest<CloseAccountResponse>;