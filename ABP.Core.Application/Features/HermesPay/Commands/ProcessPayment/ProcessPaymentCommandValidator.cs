using FluentValidation;

namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(a => a.CommerceId).GreaterThan(0).When(a => a.CommerceUserId == null);
            RuleFor(a => a.CardNumber).NotEmpty().Matches("^\\d{16}$");
            RuleFor(a => a.MonthExpirationCard).NotEmpty().Matches("^(0[1-9]|1[0-2])$");
            RuleFor(a => a.YearExpirationCard).NotEmpty().Matches("^\\d{4}$");
            RuleFor(a => a.Cvc).NotEmpty().Matches("^\\d{3}$");
            RuleFor(a => a.TransactionAmount).GreaterThan(0);
        }
    }
}
