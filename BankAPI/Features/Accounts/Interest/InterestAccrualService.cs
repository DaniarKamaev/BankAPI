using BankAPI.Shared;
using BankAPI.Shared.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Features.Accounts.Interest;

public interface IInterestAccrualService
{
    void Initialize();
    Task AccrueInterest();
}

public class InterestAccrualService : IInterestAccrualService
{
    private readonly BankDbContext _db;
    private readonly IRecurringJobManager _recurringJobManager;

    public InterestAccrualService(BankDbContext db, IRecurringJobManager recurringJobManager)
    {
        _db = db;
        _recurringJobManager = recurringJobManager;
    }

    public void Initialize()
    {
        _recurringJobManager.AddOrUpdate(
            "accrue-interest",
            () => AccrueInterest(),
            Cron.Daily);
    }

    public async Task AccrueInterest()
    {
        var accounts = await _db.Accounts
            .Where(a => a.Type != AccountType.Checking && a.InterestRate.HasValue)
            .ToListAsync();

        foreach (var account in accounts)
        {
            var interest = account.Balance * account.InterestRate.Value / 365m;
            account.Balance += interest;

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                Amount = interest,
                Currency = (CurrencyType)account.Currency,
                Type = TransactionType.Debit,
                Description = $"Daily interest accrual at rate {account.InterestRate.Value:P2}",
                DateTime = DateTime.UtcNow
            };

            await _db.Transactions.AddAsync(transaction);
        }

        await _db.SaveChangesAsync();
    }
}