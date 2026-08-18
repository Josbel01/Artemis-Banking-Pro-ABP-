using ABP.Core.Application.Exceptions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Swashbuckle.AspNetCore.Annotations;

namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommand : IRequest
    {
        public int CommerceId { get; set; }
        public string? CommerceUserId { get; set; }
        
        [SwaggerParameter(Description = "The 16-digit credit card number")]
        public string CardNumber { get; set; } = string.Empty;
        
        [SwaggerParameter(Description = "The 2-digit expiration month (MM)")]
        public string MonthExpirationCard { get; set; } = string.Empty;
        
        [SwaggerParameter(Description = "The 4-digit expiration year (YYYY)")]
        public string YearExpirationCard { get; set; } = string.Empty;
        
        [SwaggerParameter(Description = "The 3-digit security code")]
        public string Cvc { get; set; } = string.Empty;
        
        [SwaggerParameter(Description = "The amount to be processed")]
        public decimal TransactionAmount { get; set; }
    }

    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand>
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

        public async Task Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.CardNumber) || command.CardNumber.Length != 16 || !command.CardNumber.All(char.IsDigit))
                throw new ApiException("El número de tarjeta debe tener exactamente 16 dígitos.");

            if (command.TransactionAmount <= 0)
                throw new ApiException("El monto de la transacción debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(command.Cvc) || command.Cvc.Length != 3 || !command.Cvc.All(char.IsDigit))
                throw new ApiException("El CVC debe tener exactamente 3 dígitos.");

            if (!int.TryParse(command.MonthExpirationCard, out var month) || month is < 1 or > 12 ||
                !int.TryParse(command.YearExpirationCard, out var year) || year < DateTime.UtcNow.Year)
                throw new ApiException("La fecha de expiración de la tarjeta es inválida.");

            Commerce commerce = null;
            if (!string.IsNullOrEmpty(command.CommerceUserId))
            {
                commerce = await _commerceRepository.GetByUserIdAsync(command.CommerceUserId);
            }
            else
            {
                commerce = await _commerceRepository.GetByIdAsync(command.CommerceId);
            }

            if (commerce == null)
                throw new ApiException("El comercio no existe."); 
            
            if (!commerce.IsActive)
                throw new ApiException("El comercio no existe o está inactivo.");

            if (string.IsNullOrEmpty(commerce.UserId))
                throw new ApiException("El comercio no tiene un usuario asociado.");

            var principalAccount = await _savingAccountRepository.GetPrincipalAccountByClientIdAsync(commerce.UserId);
            if (principalAccount == null || principalAccount.Status != SavingAccountStatus.Active)
                throw new ApiException("El comercio no tiene una cuenta principal activa para recibir los fondos.");

            var creditCard = await _creditCardRepository.GetByCardNumberAsync(command.CardNumber);
            if (creditCard == null)
                throw new ApiException("La tarjeta no existe.");
                
            if (creditCard.Status != CreditCardStatus.Active)
                throw new ApiException("La tarjeta está inactiva o cancelada.");

            var expectedExpiration = $"{month:D2}/{year % 100:D2}";
            if (creditCard.ExpirationDate != expectedExpiration)
                throw new ApiException("Los datos de la tarjeta (fecha de expiración) son incorrectos.");

            if (new DateTime(year, month, 1).AddMonths(1) <= DateTime.UtcNow)
                throw new ApiException("La tarjeta está vencida.");

            string inputHash = ComputeSha256Hash(command.Cvc);
            if (creditCard.Cvc != inputHash)
                throw new ApiException("Los datos de la tarjeta (CVC) son incorrectos.");

            decimal availableCredit = creditCard.CreditLimit - creditCard.CurrentDebt;

            if (command.TransactionAmount > availableCredit)
            {
                var rejectedTrans = new CardTransaction
                {
                    CreditCardId = creditCard.Id,
                    CommerceId = commerce.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = command.TransactionAmount,
                    CommerceName = commerce.Name,
                    Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Rejected
                };
                await _cardTransactionRepository.AddAsync(rejectedTrans);
                
                throw new ApiException("El monto de la transacción excede el crédito disponible de la tarjeta.");
            }

            string lastFour = command.CardNumber.Substring(command.CardNumber.Length - 4);

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                creditCard.CurrentDebt += command.TransactionAmount;
                await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

                principalAccount.Balance += command.TransactionAmount;
                await _savingAccountRepository.UpdateAsync(principalAccount.Id, principalAccount);

                var approvedTrans = new CardTransaction
                {
                    CreditCardId = creditCard.Id,
                    CommerceId = commerce.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = command.TransactionAmount,
                    CommerceName = commerce.Name,
                    Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
                };
                await _cardTransactionRepository.AddAsync(approvedTrans);

                var savingTrans = new ABP.Core.Domain.Entities.Transaction
                {
                    SavingAccountId = principalAccount.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = command.TransactionAmount,
                    Type = TransactionType.Credit,
                    Beneficiary = principalAccount.AccountNumber,
                    Origin = lastFour,
                    Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
                };
                await _transactionRepository.AddAsync(savingTrans);

                scope.Complete();
            }

            try
            {
                var emailDto = new ABP.Core.Application.Dtos.Email.EmailRequestDto
                {
                    To = commerce.Email,
                    Subject = $"Pago recibido a través de tarjeta {lastFour}",
                    HtmlBody = $"Se ha procesado exitosamente un pago por el monto de {command.TransactionAmount} usando Hermes Pay. Fecha: {DateTime.Now}"
                };
                await _emailService.SendAsync(emailDto);
            }
            catch { }
        }

        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
