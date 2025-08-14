using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Features.Accounts.Transfer;

public static class TransferEndpoint
{
    public static void MapTransferEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transfer", async ([FromBody] TransferRequest request, IMediator mediator) =>
        {
            var response = await mediator.Send(request);
            return Results.Ok(response);
        }).RequireAuthorization();
    }
}