using FluentValidation;

namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(a => a.CommerceId).GreaterThan(0);
            RuleFor(a => a.CardNumber).NotEmpty().Length(16);
            RuleFor(a => a.ExpirationDate).NotEmpty();
            RuleFor(a => a.Cvc).NotEmpty().Length(3);
            RuleFor(a => a.Amount).GreaterThan(0);
        }
    }
}
