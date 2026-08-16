using ABP.Core.Application.Exceptions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommand : IRequest<int>
    {
        public int CommerceId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string ExpirationDate { get; set; } = string.Empty;
        public string Cvc { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, int>
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ICommerceRepository _commerceRepository;
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;

        public ProcessPaymentCommandHandler(
            ICreditCardRepository creditCardRepository,
            ICommerceRepository commerceRepository,
            ISavingAccountRepository savingAccountRepository,
            ICardTransactionRepository cardTransactionRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService)
        {
            _creditCardRepository = creditCardRepository;
            _commerceRepository = commerceRepository;
            _savingAccountRepository = savingAccountRepository;
            _cardTransactionRepository = cardTransactionRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
        }

        public async Task<int> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
        {
            var commerce = await _commerceRepository.GetByIdAsync(command.CommerceId);
            if (commerce == null || !commerce.IsActive)
                throw new ApiException("El comercio no existe o está inactivo.");

            var creditCard = await _creditCardRepository.GetByCardNumberAsync(command.CardNumber);
            if (creditCard == null || creditCard.Status != CreditCardStatus.Active)
                throw new ApiException("La tarjeta no existe o está inactiva.");

            if (creditCard.ExpirationDate != command.ExpirationDate || creditCard.Cvc != command.Cvc)
                throw new ApiException("Los datos de la tarjeta (fecha de expiración o CVC) son incorrectos.");

            decimal availableCredit = creditCard.CreditLimit - creditCard.CurrentDebt;

            if (command.Amount > availableCredit)
            {
                var rejectedTrans = new CardTransaction
                {
                    CreditCardId = creditCard.Id,
                    CommerceId = commerce.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = command.Amount,
                    CommerceName = commerce.Name,
                    Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Rejected
                };
                await _cardTransactionRepository.AddAsync(rejectedTrans);
                
                throw new ApiException("Fondos insuficientes o límite de crédito excedido.");
            }

            if (string.IsNullOrEmpty(commerce.UserId))
                throw new ApiException("El comercio no tiene un usuario asignado para recibir los fondos.");

            var principalAccount = await _savingAccountRepository.GetPrincipalAccountByClientIdAsync(commerce.UserId);
            if (principalAccount == null)
                throw new ApiException("El comercio no tiene una cuenta principal para recibir los fondos.");

            int newTransactionId;

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                creditCard.CurrentDebt += command.Amount;
                await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

                principalAccount.Balance += command.Amount;
                await _savingAccountRepository.UpdateAsync(principalAccount.Id, principalAccount);

                var approvedTrans = new CardTransaction
                {
                    CreditCardId = creditCard.Id,
                    CommerceId = commerce.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = command.Amount,
                    CommerceName = commerce.Name,
                    Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
                };
                var savedCardTrans = await _cardTransactionRepository.AddAsync(approvedTrans);
                newTransactionId = savedCardTrans.Id;

                var savingTrans = new ABP.Core.Domain.Entities.Transaction
                {
                    SavingAccountId = principalAccount.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = command.Amount,
                    Type = TransactionType.Credit,
                    Beneficiary = commerce.Name,
                    Origin = "Hermes Pay",
                    Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
                };
                await _transactionRepository.AddAsync(savingTrans);

                scope.Complete();
            }

            try
            {
                await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                {
                    To = commerce.Email,
                    Subject = "Nuevo Pago Recibido - Hermes Pay",
                    HtmlBody = "Se ha procesado exitosamente un pago por el monto de $" + command.Amount + " usando Hermes Pay."
                });
            }
            catch { }

            return newTransactionId;
        }
    }
}
