using FluentValidation;

namespace ABP.Core.Application.Features.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQueryValidator : AbstractValidator<GetLoanByIdQuery>
    {
        public GetLoanByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty().NotNull();
        }
    }
}
