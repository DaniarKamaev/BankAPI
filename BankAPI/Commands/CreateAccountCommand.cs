using BankAPI.Models;
using MediatR;

namespace BankAPI.Commands
{
    public record CreateAccountCommand(string OwnerId, string Currency, decimal Balance) : IRequest<Account>;
}
