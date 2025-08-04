using BankAPI.Shared.Models;

namespace BankAPI.Shared;

public class BankDbContext
{
    public List<Account> Accounts { get; } = new();

    // В реальном приложении здесь был бы DbContext и методы SaveChangesAsync
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}