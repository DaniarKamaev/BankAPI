using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Features.Accounts.Create;

public static class CreateAccountEndpoint
{
    public static void MapCreateAccountEndpoint(this IEndpointRouteBuilder app)
    {
     app.MapPost("api/accounts", async (
     [FromBody] CreateAccount request,
     IMediator mediator,
     CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await mediator.Send(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (ValidationException ex)
            {
                return Results.BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).RequireAuthorization();
    }
}