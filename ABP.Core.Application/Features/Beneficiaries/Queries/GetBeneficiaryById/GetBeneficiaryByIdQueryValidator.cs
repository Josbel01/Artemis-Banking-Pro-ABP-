using FluentValidation;

namespace ABP.Core.Application.Features.Beneficiaries.Queries.GetBeneficiaryById
{
    public class GetBeneficiaryByIdQueryValidator : AbstractValidator<GetBeneficiaryByIdQuery>
    {
        public GetBeneficiaryByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("El ID del beneficiario debe ser mayor a 0.");
        }
    }
}