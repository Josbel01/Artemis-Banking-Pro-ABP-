using FluentValidation;

namespace ABP.Core.Application.Features.Beneficiaries.Commands.UpdateBeneficiary 
{ 
    public class UpdateBeneficiaryCommandValidator : AbstractValidator<UpdateBeneficiaryCommand>
    {
        public UpdateBeneficiaryCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El Id del beneficiario es requerido.");

            RuleFor(x => x.BeneficiaryAccountNumber)
                .NotEmpty().WithMessage("El número de cuenta es requerido.")
                .Length(9).WithMessage("El número de cuenta debe tener exactamente 9 dígitos.");
        }
    }
}
