using MediatR;

namespace BankAPI.Features.Accounts.Transfer;

public record TransferRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount)
    : IRequest<TransferResponse>;