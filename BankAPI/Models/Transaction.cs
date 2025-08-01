namespace BankAPI.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid accountId { get; set; }
        public Guid? counterpartyAccountId { get; set; }
        public decimal sum {  get; set; }
        public Currency currency { get; set; }
        public Type type { get; set; }
        public String? message { get; set; }
        public DateTime dateTime { get; set; }

    }
}
