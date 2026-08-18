using FluentValidation;

namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(a => a.CommerceId).GreaterThan(0).When(a => a.CommerceUserId == null);
            RuleFor(a => a.CardNumber).NotEmpty().Length(16);
            RuleFor(a => a.MonthExpirationCard).NotEmpty().Length(2);
            RuleFor(a => a.YearExpirationCard).NotEmpty().Length(4);
            RuleFor(a => a.Cvc).NotEmpty().Length(3);
            RuleFor(a => a.TransactionAmount).GreaterThan(0);
        }
    }
}
