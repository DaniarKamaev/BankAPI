using BankAPI.Shared;
using BankAPI.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Features.Accounts.GetAccounts;


public class GetAccountsHandler(BankDbContext db) : IRequestHandler<GetAccountsQuery, IEnumerable<Account>>
{
    public async Task<IEnumerable<Account>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"Запрос учетных записей на идентификатор владельца: {request.OwnerId}");
            var accounts = await db.Accounts
                .Where(a => a.OwnerId == request.OwnerId)
                .Include(a => a.Transactions) // Жадно загружаем транзакции
                .ToListAsync(cancellationToken);
            Console.WriteLine($"Найдены {accounts.Count} аккаунтов");
            return accounts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в GetAccountsHandler: {ex.Message}");
            throw;
        }
    }
}