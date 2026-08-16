using FluentValidation;
using ABP.Core.Domain.Interfaces;

namespace ABP.Core.Application.Features.LoanInstallments.Commands.CreateLoanInstallment
{
    public class CreateLoanInstallmentCommandValidator : AbstractValidator<CreateLoanInstallmentCommand>
    {
        private readonly ILoanRepository _loanRepository;

        public CreateLoanInstallmentCommandValidator(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;

            RuleFor(x => x.LoanId)
                .GreaterThan(0).WithMessage("Loan ID must be greater than 0.")
                .MustAsync(async (id, cancellation) => 
                {
                    var loanExists = await _loanRepository.GetByIdAsync(id);
                    return loanExists != null;  
                }).WithMessage("Loan does not exist.");

            RuleFor(x => x.InstallmentNumber)
                .GreaterThan(0).WithMessage("Installment number must be greater than 0.");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required.");

            RuleFor(x => x.InstallmentAmount)
                .GreaterThan(0).WithMessage("Installment amount must be greater than 0.");

            RuleFor(x => x.InterestAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Interest amount cannot be negative.");

            RuleFor(x => x.CapitalAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Capital amount cannot be negative.");
        }
    }
}