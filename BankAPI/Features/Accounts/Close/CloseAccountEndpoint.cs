using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Features.Accounts.Close;

public static class CloseAccountEndpoint
{
    public static void MapCloseAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/accounts/{accountId}/close", async (
            [FromRoute] Guid accountId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var request = new CloseAccount(accountId);
            var result = await mediator.Send(request, cancellationToken);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}