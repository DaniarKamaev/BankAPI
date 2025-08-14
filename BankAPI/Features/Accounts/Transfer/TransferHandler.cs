using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Features.Accounts.Transfer;

public class TransferHandler(BankDbContext db) : IRequestHandler<TransferRequest, TransferResponse>
{
    public async Task<TransferResponse> Handle(TransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var fromAccount = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.FromAccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Account with Id {request.FromAccountId} not found.");

            var toAccount = await db.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.ToAccountId, cancellationToken)
                ?? throw new InvalidOperationException($"Account with Id {request.ToAccountId} not found.");

            if (fromAccount.Balance < request.Amount)
                throw new InvalidOperationException("Insufficient balance for transfer.");

            // Проверяем, что валюты счетов совпадают
            if (fromAccount.Currency != toAccount.Currency)
                throw new InvalidOperationException("Accounts must have the same currency for transfer.");

            fromAccount.Balance -= request.Amount;
            toAccount.Balance += request.Amount;

            var withdrawal = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.FromAccountId,
                CounterpartyAccountId = request.ToAccountId,
                Amount = -request.Amount,
                Currency = (CurrencyType)fromAccount.Currency,
                Type = TransactionType.Credit,
                Description = $"Transfer to Account {request.ToAccountId}",
                DateTime = DateTime.UtcNow
            };

            var deposit = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = request.ToAccountId,
                CounterpartyAccountId = request.FromAccountId,
                Amount = request.Amount,
                Currency = (CurrencyType)toAccount.Currency,
                Type = TransactionType.Debit,
                Description = $"Transfer from Account {request.FromAccountId}",
                DateTime = DateTime.UtcNow
            };

            await db.Transactions.AddRangeAsync([withdrawal, deposit], cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            Console.WriteLine($"Transferred {request.Amount} from AccountId: {request.FromAccountId} to AccountId: {request.ToAccountId}");
            return new TransferResponse(
                "Перевод выполнен",
                fromAccount.Balance,
                toAccount.Balance);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TransferHandler: {ex.Message}");
            throw;
        }
    }
}