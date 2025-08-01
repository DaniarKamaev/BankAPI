using System;

namespace BankAPI.Models
{

    namespace BankAPI.Models
    {
        public class Account
        {
            public Guid Id { get; set; }
            public Guid OwnerId { get; set; }
            public AccountType Type { get; set; }
            public CurrencyType Currency { get; set; }
            public decimal Balance { get; set; }
            public decimal? InterestRate { get; set; } // только для вкладов/кредитов
            public DateTime OpenDate { get; set; }
            public DateTime? CloseDate { get; set; }
            public List<Transaction> Transactions { get; set; } = new();
        }

        public enum AccountType
        { 
            Checking,  // Текущий счёт
            Deposit,   // Вклад
            Credit,     // Кредитный
            Savings
        }

        public enum CurrencyType
        {
            RUB,
            USD,
            EUR
        }
    }




}
