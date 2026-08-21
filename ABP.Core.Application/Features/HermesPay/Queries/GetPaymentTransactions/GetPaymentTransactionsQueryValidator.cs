using FluentValidation;

namespace ABP.Core.Application.Features.HermesPay.Queries.GetPaymentTransactions
{
    public class GetPaymentTransactionsQueryValidator : AbstractValidator<GetPaymentTransactionsQuery>
    {
        public GetPaymentTransactionsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 20).WithMessage("El tamaño de página debe estar entre 1 y 20.");

            RuleFor(x => x.CommerceId)
                .GreaterThan(0).WithMessage("Commerce ID must be greater than 0.");
        }
    }
}
