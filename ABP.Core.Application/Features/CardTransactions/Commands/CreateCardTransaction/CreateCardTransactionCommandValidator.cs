using FluentValidation;
using ABP.Core.Domain.Interfaces;

namespace ABP.Core.Application.Features.CardTransactions.Commands.CreateCardTransaction
{
    public class CreateCardTransactionCommandValidator : AbstractValidator<CreateCardTransactionCommand>
    {
        private readonly ICreditCardRepository _creditCardRepository;
        
        public CreateCardTransactionCommandValidator(ICreditCardRepository creditCardRepository)
        {
            _creditCardRepository = creditCardRepository;

            RuleFor(x => x.CreditCardId)
                .GreaterThan(0).WithMessage("Credit Card ID must be greater than 0.")
                .MustAsync(async (id, cancellation) => 
                {
                    var creditCardExists = await _creditCardRepository.GetByIdAsync(id);
                    return creditCardExists != null;  
                }).WithMessage("Credit Card does not exist.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Transaction amount must be greater than 0.");
        }
    }
}