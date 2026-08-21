using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Exceptions;
using ABP.Core.Application.Helpers;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand>
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ICommerceRepository _commerceRepository;
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IEmailService _emailService;
        private readonly IBaseAccountService _accountService;
        private readonly ILogger<ProcessPaymentCommandHandler> _logger;

        public ProcessPaymentCommandHandler(
            ICreditCardRepository creditCardRepository,
            ICommerceRepository commerceRepository,
            ISavingAccountRepository savingAccountRepository,
            ICardTransactionRepository cardTransactionRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IBaseAccountService accountService,
            ILoggerFactory loggerFactory)
        {
            _creditCardRepository = creditCardRepository;
            _commerceRepository = commerceRepository;
            _savingAccountRepository = savingAccountRepository;
            _cardTransactionRepository = cardTransactionRepository;
            _transactionRepository = transactionRepository;
            _emailService = emailService;
            _accountService = accountService;
            _logger = loggerFactory.CreateLogger<ProcessPaymentCommandHandler>();
        }

        public async Task Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing payment for Commerce ID: {CommerceId}, Amount: {Amount}", command.CommerceId, command.TransactionAmount);

            ValidateInput(command);

            var commerce = await GetCommerceAsync(command);
            var principalAccount = await GetCommerceAccountAsync(commerce);
            var creditCard = await GetCreditCardAsync(command);
            ValidateExpiration(creditCard, command);

            string inputHash = PasswordEncryptation.ComputeSha256Hash(command.Cvc);
            if (creditCard.Cvc != inputHash)
                throw new ApiException("Los datos de la tarjeta (CVC) son incorrectos.");

            decimal availableCredit = creditCard.CreditLimit - creditCard.CurrentDebt;
            string lastFour = command.CardNumber[^4..];

            if (command.TransactionAmount > availableCredit)
            {
                await HandleRejectedPaymentAsync(creditCard, commerce, command.TransactionAmount, availableCredit, lastFour);
                throw new ApiException("El monto de la transacción excede el crédito disponible de la tarjeta.");
            }

            await ProcessApprovedPaymentAsync(creditCard, commerce, principalAccount, command.TransactionAmount, lastFour);
        }

        #region Validation

        private static void ValidateInput(ProcessPaymentCommand command)
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
        }

        private async Task<Commerce> GetCommerceAsync(ProcessPaymentCommand command)
        {
            Commerce commerce;

            if (!string.IsNullOrEmpty(command.CommerceUserId))
                commerce = await _commerceRepository.GetByUserIdAsync(command.CommerceUserId);
            else
                commerce = await _commerceRepository.GetByIdAsync(command.CommerceId);

            if (commerce == null)
                throw new ApiException("El comercio no existe.");

            if (!commerce.IsActive)
                throw new ApiException("El comercio no existe o está inactivo.");

            if (string.IsNullOrEmpty(commerce.UserId))
                throw new ApiException("El comercio no tiene un usuario asociado.");

            return commerce;
        }

        private async Task<SavingAccount> GetCommerceAccountAsync(Commerce commerce)
        {
            var account = await _savingAccountRepository.GetPrincipalAccountByClientIdAsync(commerce.UserId);

            if (account == null || account.Status != SavingAccountStatus.Active)
                throw new ApiException("El comercio no tiene una cuenta principal activa para recibir los fondos.");

            return account;
        }

        private async Task<CreditCard> GetCreditCardAsync(ProcessPaymentCommand command)
        {
            var card = await _creditCardRepository.GetByCardNumberAsync(command.CardNumber);

            if (card == null)
                throw new ApiException("La tarjeta no existe.");

            if (card.Status != CreditCardStatus.Active)
                throw new ApiException("La tarjeta está inactiva o cancelada.");

            return card;
        }

        private static void ValidateExpiration(CreditCard card, ProcessPaymentCommand command)
        {
            if (!int.TryParse(command.MonthExpirationCard, out var month) ||
                !int.TryParse(command.YearExpirationCard, out var year))
                throw new ApiException("La fecha de expiración de la tarjeta es inválida.");

            if (card.ExpirationDate.Month != month || card.ExpirationDate.Year != year)
                throw new ApiException("Los datos de la tarjeta (fecha de expiración) son incorrectos.");

            if (card.ExpirationDate.AddMonths(1) <= DateTime.UtcNow)
                throw new ApiException("La tarjeta está vencida.");
        }

        #endregion

        #region Payment Processing

        private async Task ProcessApprovedPaymentAsync(
            CreditCard creditCard, Commerce commerce, SavingAccount principalAccount,
            decimal amount, string lastFour)
        {
            creditCard.CurrentDebt += amount;
            await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

            principalAccount.Balance += amount;
            await _savingAccountRepository.UpdateAsync(principalAccount.Id, principalAccount);

            await _cardTransactionRepository.AddAsync(new CardTransaction
            {
                CreditCardId = creditCard.Id,
                CommerceId = commerce.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = amount,
                CommerceName = commerce.Name,
                Status = TransactionStatus.Approved
            });

            await _transactionRepository.AddAsync(new Transaction
            {
                SavingAccountId = principalAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = amount,
                Type = TransactionType.Credit,
                Beneficiary = principalAccount.AccountNumber,
                Origin = lastFour,
                Status = TransactionStatus.Approved
            });

            _logger.LogInformation("Payment of RD${Amount} for Commerce {CommerceId} processed successfully.", amount, commerce.Id);

            await SendApprovedEmailsAsync(creditCard, commerce, amount, lastFour);
        }

        private async Task HandleRejectedPaymentAsync(
            CreditCard creditCard, Commerce commerce,
            decimal amount, decimal availableCredit, string lastFour)
        {
            await _cardTransactionRepository.AddAsync(new CardTransaction
            {
                CreditCardId = creditCard.Id,
                CommerceId = commerce.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = amount,
                CommerceName = commerce.Name,
                Status = TransactionStatus.Rejected
            });

            await SendRejectionEmailAsync(creditCard, commerce, amount, availableCredit, lastFour);
        }

        #endregion

        #region Emails

        private async Task SendApprovedEmailsAsync(CreditCard creditCard, Commerce commerce, decimal amount, string lastFour)
        {
            string dateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            try
            {
                var cardOwner = await GetUserByIdAsync(creditCard.ClientId);
                if (cardOwner != null)
                {
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = cardOwner.Email,
                        Subject = $"Consumo realizado con la tarjeta {lastFour}",
                        HtmlBody = EmailTemplates.HermesPayCardHolderApproved(
                            cardOwner.FirstName, commerce.Name, amount,
                            creditCard.CurrentDebt, lastFour, dateTime)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send card holder email for payment.");
            }

            try
            {
                var commerceUser = await GetUserByIdAsync(commerce.UserId);
                if (commerceUser != null)
                {
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = commerceUser.Email,
                        Subject = $"Pago recibido a través de tarjeta {lastFour}",
                        HtmlBody = EmailTemplates.HermesPayCommerceReceived(
                            commerce.Name, amount, lastFour, dateTime)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send commerce email for payment.");
            }
        }

        private async Task SendRejectionEmailAsync(CreditCard creditCard, Commerce commerce, decimal amount, decimal availableCredit, string lastFour)
        {
            try
            {
                var cardOwner = await GetUserByIdAsync(creditCard.ClientId);
                if (cardOwner != null)
                {
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = cardOwner.Email,
                        Subject = $"Pago rechazado en {commerce.Name} - Tarjeta {lastFour}",
                        HtmlBody = EmailTemplates.HermesPayCardHolderRejected(
                            cardOwner.FirstName, commerce.Name, amount,
                            availableCredit, DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send rejection email.");
            }
        }

        private async Task<ABP.Core.Application.Dtos.User.UserDto?> GetUserByIdAsync(string userId)
        {
            try { return await _accountService.GetUserById(userId); }
            catch { return null; }
        }

        #endregion
    }
}
