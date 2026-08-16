using FluentValidation;

namespace ABP.Core.Application.Features.Beneficiaries.Commands.DeleteBeneficiary 
{ 
    public class DeleteBeneficiaryCommandValidator : AbstractValidator<DeleteBeneficiaryCommand>
    {
        public DeleteBeneficiaryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("Beneficiary ID is required.")
                .GreaterThan(0).WithMessage("Beneficiary ID must be greater than 0.");
        }
    } 
}