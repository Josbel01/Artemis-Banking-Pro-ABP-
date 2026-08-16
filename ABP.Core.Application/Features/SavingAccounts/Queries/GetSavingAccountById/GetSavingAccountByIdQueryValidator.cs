using FluentValidation;

namespace ABP.Core.Application.Features.SavingAccounts.Queries.GetSavingAccountById
{
    public class GetSavingAccountByIdQueryValidator : AbstractValidator<GetSavingAccountByIdQuery>
    {
        public GetSavingAccountByIdQueryValidator()
        {
            RuleFor(a => a.Id)
                .GreaterThan(0).WithMessage("SavingAccount ID must be greater than 0.")
                .NotNull().WithMessage("SavingAccount ID is required.");
        }
    }
}
