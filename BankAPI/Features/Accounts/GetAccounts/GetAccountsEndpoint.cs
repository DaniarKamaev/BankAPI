using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Features.Accounts.GetAccounts;

public static class GetAccountsEndpoint
{
    public static void MapGetAccountsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/owner/{ownerId}", async (Guid ownerId, IMediator mediator) =>
        {
            var accounts = await mediator.Send(new GetAccountsQuery(ownerId));
            return Results.Ok(accounts);
        }).RequireAuthorization();
    }
}