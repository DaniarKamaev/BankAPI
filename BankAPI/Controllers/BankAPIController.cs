using BankAPI.Models;
using BankAPI.Models.BankAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;


namespace BankAPI.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private static List<Account> _accounts = new();

        // Создать счёт
        [HttpPost]
        public IActionResult CreateAccount([FromBody] Account account)
        {
            account.Id = Guid.NewGuid();
            account.OpenDate = DateTime.Now;
        
                account.InterestRate = (decimal?)0.03;
            }

            return Ok(new
            {
                Message = $"Счёт {account.Type} открыт",
                AccountId = account.Id
            });
        }

        // Получить все счета клиента
        [HttpGet("owner/{ownerId}")]
        public IActionResult GetAccounts(Guid ownerId)
        {
            var accounts = _accounts.Where(a => a.OwnerId == ownerId).ToList();
            return Ok(accounts);
        }

        // Пополнить счёт
        [HttpPost("{accountId}/deposit")]
        public IActionResult Deposit(Guid accountId, [FromBody] decimal amount)
        {
            var account = _accounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return NotFound();

            account.Balance += amount;
            account.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = amount,
                Currency = account.Currency,
                Type = TransactionType.Credit,
                Description = "Пополнение наличными",
                DateTime = DateTime.Now
            });

            return Ok($"Счёт пополнен на {amount} {account.Currency}");
        }

        // Перевод между счетами
        [HttpPost("transfer")]
        public IActionResult Transfer([FromBody] TransferRequest request)
        {
            var fromAccount = _accounts.FirstOrDefault(a => a.Id == request.FromAccountId);
            var toAccount = _accounts.FirstOrDefault(a => a.Id == request.ToAccountId);

            if (fromAccount == null || toAccount == null)
                return BadRequest("Один из счетов не найден");

            if (fromAccount.Balance < request.Amount)
                return BadRequest("Недостаточно средств");

            if (fromAccount.Currency != toAccount.Currency)
            {
                return BadRequest("Разные Валюты");
            }

            // Списание
            fromAccount.Balance -= request.Amount;
            fromAccount.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = fromAccount.Id,
                CounterpartyAccountId = toAccount.Id,
                Amount = request.Amount,
                Currency = fromAccount.Currency,
                Type = TransactionType.Debit,
                Description = $"Перевод на счёт {toAccount.Id}",
                DateTime = DateTime.Now
            });

            // Зачисление
            toAccount.Balance += request.Amount;
            toAccount.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = toAccount.Id,
                CounterpartyAccountId = fromAccount.Id,
                Amount = request.Amount,
                Currency = toAccount.Currency,
                Type = TransactionType.Credit,
                Description = $"Перевод со счёта {fromAccount.Id}",
                DateTime = DateTime.Now
            });

            return Ok("Перевод выполнен успешно");
        }
    }

    
}


