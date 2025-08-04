using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Features.Accounts.Transfer;

public static class TransferEndpoint
{
    public static void MapTransferEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/accounts/transfer", async (
            [FromBody] TransferRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(request, cancellationToken);
            return Results.Ok(result);
        });
    }
}