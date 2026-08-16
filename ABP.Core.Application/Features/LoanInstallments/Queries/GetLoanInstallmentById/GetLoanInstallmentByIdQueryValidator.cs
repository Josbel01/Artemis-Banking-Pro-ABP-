using FluentValidation;

namespace ABP.Core.Application.Features.LoanInstallments.Queries.GetLoanInstallmentById
{
    public class GetLoanInstallmentByIdQueryValidator : AbstractValidator<GetLoanInstallmentByIdQuery>
    {
        public GetLoanInstallmentByIdQueryValidator()
        {
            RuleFor(a => a.Id)
                .GreaterThan(0).WithMessage("LoanInstallment ID must be greater than 0.")
                .NotNull().WithMessage("LoanInstallment ID is required.");
        }
    }
}
