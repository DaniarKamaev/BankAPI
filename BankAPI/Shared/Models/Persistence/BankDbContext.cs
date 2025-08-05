using BankAPI.Shared.Models;

namespace BankAPI.Shared;

public class BankDbContext
{
    public List<Account> Accounts { get; } = new();
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}