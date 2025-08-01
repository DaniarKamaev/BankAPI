namespace BankAPI.Models
{
    namespace BankAPI.Models
    {
        public class Transaction
        {
            public Guid Id { get; set; }
            public Guid AccountId { get; set; }
            public Guid? CounterpartyAccountId { get; set; }
            public decimal Amount { get; set; }
            public CurrencyType Currency { get; set; }
            public TransactionType Type { get; set; }
            public string Description { get; set; }
            public DateTime DateTime { get; set; }
        }

        public enum TransactionType
        {
            Credit, // Зачисление
            Debit   // Списание
        }
    }
    public class TransferRequest
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
