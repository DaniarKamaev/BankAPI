// Features/Accounts/Transfer/TransferResponse.cs
namespace BankAPI.Features.Accounts.Transfer;

public record TransferResponse(string Message, decimal FromAccountBalance, decimal ToAccountBalance);