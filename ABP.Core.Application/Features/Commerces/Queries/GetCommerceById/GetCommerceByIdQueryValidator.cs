using FluentValidation;

namespace ABP.Core.Application.Features.Commerces.Queries.GetCommerceById
{
    public class GetCommerceByIdQueryValidator : AbstractValidator<GetCommerceByIdQuery>
    {
        public GetCommerceByIdQueryValidator()
        {
            RuleFor(a => a.Id)
                .GreaterThan(0).WithMessage("Commerce ID must be greater than 0.")
                .NotNull().WithMessage("Commerce ID is required.");
        }
    }
}
