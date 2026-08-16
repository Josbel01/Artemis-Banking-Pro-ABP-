using FluentValidation;

namespace ABP.Core.Application.Features.Transactions.Queries.GetTransactionById
{
    public class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
    {
        public GetTransactionByIdQueryValidator()
        {
            RuleFor(a => a.Id)
                .GreaterThan(0).WithMessage("Transaction ID must be greater than 0.")
                .NotNull().WithMessage("Transaction ID is required.");
        }
    }
}
