using FluentValidation;

namespace BankAPI.Features.Accounts.Transfer;

public class TransferValidator : AbstractValidator<TransferRequest>
{
    public TransferValidator()
    {
        RuleFor(x => x.FromAccountId).NotEmpty().WithMessage("FromAccountId is required.");
        RuleFor(x => x.ToAccountId).NotEmpty().WithMessage("ToAccountId is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be positive.");
        RuleFor(x => x.FromAccountId)
            .NotEqual(x => x.ToAccountId)
            .WithMessage("FromAccountId and ToAccountId must be different.");
    }
}