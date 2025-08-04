using BankAPI.Shared.Models;
using FluentValidation;

namespace BankAPI.Features.Accounts.Create;

public class CreateAccountValidator : AbstractValidator<CreateAccount>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId обязателен");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Недопустимый тип счёта");
        RuleFor(x => x.Currency).IsInEnum();
        RuleFor(x => x.Balance).GreaterThanOrEqualTo(0).WithMessage("Баланс не может быть отрицательным");
        RuleFor(x => x.InterestRate)
            .GreaterThan(0)
            .When(x => x.Type != AccountType.Checking)
            .WithMessage("Для вклада/кредита должна быть указана ставка");
    }
}