using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Features.Accounts.Deposit;

public static class DepositEndpoint
{
    public static void MapDepositEndpoint(this IEndpointRouteBuilder app)
    {
    app.MapPost("api/accounts/{accountId}/deposit", async (
    [FromRoute] Guid accountId,
    [FromBody] decimal amount,
    IMediator mediator,
    CancellationToken cancellationToken) =>
        {
            var request = new DepositRequest(accountId, amount);
            var result = await mediator.Send(request, cancellationToken);
            return Results.Ok(result);
        }).RequireAuthorization();
    }
}