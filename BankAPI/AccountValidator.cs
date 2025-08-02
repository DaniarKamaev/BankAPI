using BankAPI.Models.BankAPI.Models;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace BankAPI
{
    public class AccountValidator : AbstractValidator<Account>
    {
        public AccountValidator() 
        {
            RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId обязателен");
            RuleFor(x => x.Type).IsInEnum().WithMessage("Недопустимый тип счёта");
            RuleFor(x => x.Currency).IsInEnum();
            RuleFor(x => x.Balance).GreaterThanOrEqualTo(0).WithMessage("Баланс не может быть отрицательным");
            RuleFor(x => x.InterestRate).GreaterThan(0).WithMessage("Для вклада/кредита должна быть указана ставка");
        }
    }
}
