using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Features.Accounts.Close;

public class CloseAccountHandler(BankDbContext db) : IRequestHandler<CloseAccount, CloseAccountResponse>
{
    public async Task<CloseAccountResponse> Handle(CloseAccount request, CancellationToken cancellationToken)
    {
        var account = await db.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken)
            ?? throw new InvalidOperationException($"Account {request.AccountId} not found");

        if (account.IsLocked)
            throw new InvalidOperationException("Account already closed");

        account.CloseDate = DateTime.UtcNow;
        account.IsLocked = true;

        await db.SaveChangesAsync(cancellationToken);

        return new CloseAccountResponse(
            $"Account {request.AccountId} closed successfully",
            account.CloseDate.Value);
    }
}