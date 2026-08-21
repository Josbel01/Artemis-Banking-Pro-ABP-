using FluentValidation;

namespace ABP.Core.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID del usuario es requerido.");
        }
    }
}
