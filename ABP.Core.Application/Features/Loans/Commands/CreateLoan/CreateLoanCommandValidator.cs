using FluentValidation;

namespace ABP.Core.Application.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
    {
        public CreateLoanCommandValidator()
        {
            RuleFor(x => x.LoanNumber)
                .NotEmpty().WithMessage("Loan number is required.")
                .MaximumLength(50).WithMessage("Loan number must not exceed 50 characters.");

            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("Client ID is required.");

            RuleFor(x => x.AmountApproved)
                .GreaterThan(0).WithMessage("Approved amount must be greater than 0.");

            RuleFor(x => x.AnnualInterestRate)
                .GreaterThanOrEqualTo(0).WithMessage("Interest rate cannot be negative.");

            RuleFor(x => x.TermInMonths)
                .GreaterThan(0).WithMessage("Term in months must be greater than 0.");

            RuleFor(x => x.AssignedByUserId)
                .NotEmpty().WithMessage("Admin assigned ID is required.");
        }
    }
}