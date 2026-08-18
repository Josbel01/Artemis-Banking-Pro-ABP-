using FluentValidation;

namespace ABP.Core.Application.Features.CreditCards.Queries.GetCreditCardById
{
    public class GetCreditCardByIdQueryValidator : AbstractValidator<GetCreditCardByIdQuery>
    {
        public GetCreditCardByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("CreditCard ID must be greater than 0.")
                .NotNull().WithMessage("CreditCard ID is required.");
        }
    }
}
