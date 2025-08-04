namespace BankAPI.Features.Accounts.Create;

public record CreateAccountResponse(
    string Message,
    Guid AccountId);