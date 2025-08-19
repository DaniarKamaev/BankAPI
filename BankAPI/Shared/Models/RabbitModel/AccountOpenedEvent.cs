namespace BankAPI.Shared.Models.RabbitModel
{
    public record AccountOpenedEvent(
    Guid AccountId,
    Guid OwnerId,
    string Currency,
    string Type);
}
