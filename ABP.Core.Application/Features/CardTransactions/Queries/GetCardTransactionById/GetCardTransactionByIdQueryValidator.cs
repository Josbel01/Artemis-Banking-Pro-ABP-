using FluentValidation;

namespace ABP.Core.Application.Features.CardTransactions.Queries.GetCardTransactionById
{
    public class GetCardTransactionByIdQueryValidator : AbstractValidator<GetCardTransactionByIdQuery>
    {
        public GetCardTransactionByIdQueryValidator()
        {
            RuleFor(a => a.Id)
                .GreaterThan(0).WithMessage("CardTransaction ID must be greater than 0.")
                .NotNull().WithMessage("CardTransaction ID is required.");
        }
    }
}
