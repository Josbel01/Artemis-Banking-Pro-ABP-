using FluentValidation;

namespace ABP.Core.Application.Features.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQueryValidator : AbstractValidator<GetLoanByIdQuery>
    {
        public GetLoanByIdQueryValidator()
        {
            RuleFor(a => a.Id)
                .GreaterThan(0).WithMessage("Loan ID must be greater than 0.")
                .NotNull().WithMessage("Loan ID is required.");
        }
    }
}
