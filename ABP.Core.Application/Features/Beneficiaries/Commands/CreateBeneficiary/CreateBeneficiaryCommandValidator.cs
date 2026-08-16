using FluentValidation;

namespace ABP.Core.Application.Features.Beneficiaries.Commands.CreateBeneficiary 
{ 
    public class CreateBeneficiaryCommandValidator : AbstractValidator<CreateBeneficiaryCommand>
    {
        public CreateBeneficiaryCommandValidator()
        {
            RuleFor(x => x.BeneficiaryAccountNumber)
                .NotEmpty().WithMessage("El número de cuenta es requerido.")
                .Length(9).WithMessage("El número de cuenta debe tener exactamente 9 dígitos.");
        }
    }
}