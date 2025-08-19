using BankAPI.Shared.Models.Enums;
namespace BankAPI.Shared.Models
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string EventData { get; set; }
        public DateTime CreatedAt { get; set; }
        public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    }
}
