using FluentValidation;
using ABP.Core.Domain.Interfaces;

namespace ABP.Core.Application.Features.Transactions.Commands.CreateTransaction
{
    public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
    {
        private readonly ISavingAccountRepository _savingAccountRepository;

        public CreateTransactionCommandValidator(ISavingAccountRepository savingAccountRepository)
        {
            _savingAccountRepository = savingAccountRepository;

            RuleFor(x => x.SavingAccountId)
                .GreaterThan(0).WithMessage("Saving Account ID must be greater than 0.")
                .MustAsync(async (id, cancellation) => 
                {
                    var accountExists = await _savingAccountRepository.GetByIdAsync(id);
                    return accountExists != null;  
                }).WithMessage("Saving Account does not exist.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Transaction amount must be greater than 0.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid transaction type.");

            RuleFor(x => x.Beneficiary)
                .MaximumLength(150).WithMessage("Beneficiary name must not exceed 150 characters.");

            RuleFor(x => x.Origin)
                .NotEmpty().WithMessage("Transaction origin is required.")
                .MaximumLength(150).WithMessage("Origin must not exceed 150 characters.");
        }
    }
}