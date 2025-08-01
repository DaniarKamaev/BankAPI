using System;

namespace BankAPI.Models
{

    public class Account
    {
        public Guid Id { get; set; }
        public Guid ownerId { get; set; } //MARK: - Выпилить set если не понадобится 
        public Type type { get; set; }
        public Currency currency { get; set; }
        public decimal Balance { get; set; }
        public decimal? interestRate { get; set; } //Процентная ствка(для келитов и дебетов)
        public DateOnly dataOpen { get; set; }
        public DateOnly? dataClose { get; set; }
        public Transaction[]? transactionArray { get; }

    }

    public enum Type
    {
        Checking,
         Deposit,
         Credit
    }

    public enum Currency
    {
        USD, // Доллар США
        EUR, // Евро
        RUB, // Российский рубль
        JPY, // Японская йена
        GBP  // Фунт стерлингов
    }

    
}
