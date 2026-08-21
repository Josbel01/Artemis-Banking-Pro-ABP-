using FluentValidation;

namespace ABP.Core.Application.Features.Loans.Queries.GetAllLoans
{
    public class GetAllLoansQueryValidator : AbstractValidator<GetAllLoansQuery>
    {
        public GetAllLoansQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("El n\u00famero de p\u00e1gina debe ser mayor o igual a 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 20).WithMessage("El tama\u00f1o de p\u00e1gina debe estar entre 1 y 20.");
        }
    }
}
