using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Features.Accounts.GetAccounts;

public static class GetAccountsEndpoint
{
    public static void MapGetAccountsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/accounts/owner/{ownerId}", async (
    [FromRoute] Guid ownerId,
    IMediator mediator,
    CancellationToken cancellationToken) =>
        {
            var query = new GetAccountsQuery(ownerId);
            var accounts = await mediator.Send(query, cancellationToken);
            return Results.Ok(accounts);
        }).RequireAuthorization();
    }
}