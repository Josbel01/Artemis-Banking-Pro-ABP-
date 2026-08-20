using ABP.Core.Application.Exceptions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly IBaseAccountService _accountService;
        private readonly Microsoft.Extensions.Logging.ILogger<ProcessPaymentCommandHandler> _logger;

        public ProcessPaymentCommandHandler(
            ICreditCardRepository creditCardRepository,
            ICommerceRepository commerceRepository,
            ISavingAccountRepository savingAccountRepository,
            ICardTransactionRepository cardTransactionRepository,
            ITransactionRepository transactionRepository,
            IEmailService emailService,
            IBaseAccountService accountService,
            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
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

            if (string.IsNullOrWhiteSpace(command.CardNumber) || command.CardNumber.Length != 16 || !command.CardNumber.All(char.IsDigit))
            {
                _logger.LogWarning("Payment failed: Invalid card number format.");
                throw new ApiException("El número de tarjeta debe tener exactamente 16 dígitos.");
            }

            if (command.TransactionAmount <= 0)
            {
                _logger.LogWarning("Payment failed: Transaction amount is zero or negative.");
                throw new ApiException("El monto de la transacción debe ser mayor que cero.");
            }

            if (string.IsNullOrWhiteSpace(command.Cvc) || command.Cvc.Length != 3 || !command.Cvc.All(char.IsDigit))
            {
                _logger.LogWarning("Payment failed: Invalid CVC format.");
                throw new ApiException("El CVC debe tener exactamente 3 dígitos.");
            }

            if (!int.TryParse(command.MonthExpirationCard, out var month) || month is < 1 or > 12 ||
                !int.TryParse(command.YearExpirationCard, out var year) || year < DateTime.UtcNow.Year)
            {
                _logger.LogWarning("Payment failed: Invalid expiration date.");
                throw new ApiException("La fecha de expiración de la tarjeta es inválida.");
            }

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
            {
                _logger.LogWarning("Payment failed: Commerce not found.");
                throw new ApiException("El comercio no existe."); 
            }
            
            if (!commerce.IsActive)
            {
                _logger.LogWarning("Payment failed: Commerce is inactive.");
                throw new ApiException("El comercio no existe o está inactivo.");
            }

            if (string.IsNullOrEmpty(commerce.UserId))
            {
                _logger.LogWarning("Payment failed: Commerce has no associated user.");
                throw new ApiException("El comercio no tiene un usuario asociado.");
            }

            var principalAccount = await _savingAccountRepository.GetPrincipalAccountByClientIdAsync(commerce.UserId);
            if (principalAccount == null || principalAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Payment failed: Commerce has no active principal account.");
                throw new ApiException("El comercio no tiene una cuenta principal activa para recibir los fondos.");
            }

            var creditCard = await _creditCardRepository.GetByCardNumberAsync(command.CardNumber);
            if (creditCard == null)
            {
                _logger.LogWarning("Payment failed: Credit card not found.");
                throw new ApiException("La tarjeta no existe.");
            }
                
            if (creditCard.Status != CreditCardStatus.Active)
            {
                _logger.LogWarning("Payment failed: Credit card is inactive.");
                throw new ApiException("La tarjeta está inactiva o cancelada.");
            }

            if (creditCard.ExpirationDate.Month != month || creditCard.ExpirationDate.Year != year)
            {
                _logger.LogWarning("Payment failed: Credit card expiration date mismatch.");
                throw new ApiException("Los datos de la tarjeta (fecha de expiración) son incorrectos.");
            }

            if (creditCard.ExpirationDate.AddMonths(1) <= DateTime.UtcNow)
            {
                _logger.LogWarning("Payment failed: Credit card is expired.");
                throw new ApiException("La tarjeta está vencida.");
            }

            string inputHash = ComputeSha256Hash(command.Cvc);
            if (creditCard.Cvc != inputHash)
            {
                _logger.LogWarning("Payment failed: Invalid CVC.");
                throw new ApiException("Los datos de la tarjeta (CVC) son incorrectos.");
            }

            decimal availableCredit = creditCard.CreditLimit - creditCard.CurrentDebt;
            string lastFour = command.CardNumber.Substring(command.CardNumber.Length - 4);

            if (command.TransactionAmount > availableCredit)
            {
                _logger.LogWarning("Payment failed: Insufficient credit limit.");
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

                // Send rejection email to card holder
                try
                {
                    var cardOwner = await GetCardOwnerAsync(creditCard.ClientId);
                    if (cardOwner != null)
                    {
                        await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                        {
                            To = cardOwner.Email,
                            Subject = $"Pago rechazado en {commerce.Name} - Tarjeta {lastFour}",
                            HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#dc2626,#ef4444);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#10060; Pago Rechazado</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{cardOwner.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Su intento de pago en <strong>{commerce.Name}</strong> fue <strong>rechazado</strong> por falta de cr&#233;dito disponible.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Comercio</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{commerce.Name}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto intentado</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#dc2626;">RD${command.TransactionAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Cr&#233;dito disponible</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">RD${availableCredit:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha y hora</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
<div style="background:#fee2e2;border-left:4px solid #dc2626;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#991b1b;font-size:13px;margin:0;">&#9888;&#65039; Si usted no reconoce esta operaci&#243;n, comun&#237;quese con la entidad bancaria.</p>
</div>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Hermes Pay</p>
</div>
</div>
</body></html>
"""
                        });
                    }
                }
                catch { }

                throw new ApiException("El monto de la transacción excede el crédito disponible de la tarjeta.");
            }

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

            _logger.LogInformation("Payment of RD${Amount} for Commerce ID: {CommerceId} processed successfully.", command.TransactionAmount, commerce.Id);

            // Send email to card holder about the purchase
            try
            {
                var cardOwner = await GetCardOwnerAsync(creditCard.ClientId);
                if (cardOwner != null)
                {
                    await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                    {
                        To = cardOwner.Email,
                        Subject = $"Consumo realizado con la tarjeta {lastFour}",
                        HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#7c3aed,#a78bfa);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128722; Compra con Hermes Pay</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{cardOwner.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha procesado una compra con tu tarjeta de cr&#233;dito en <strong>{commerce.Name}</strong>.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Comercio</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{commerce.Name}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#7c3aed;">RD${command.TransactionAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Tarjeta</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFour}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Deuda actual</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#dc2626;">RD${creditCard.CurrentDebt:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha y hora</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Hermes Pay</p>
</div>
</div>
</body></html>
"""
                    });
                }
            }
            catch { }

            // Send email to commerce about the received payment
            try
            {
                var commerceUser = await GetCardOwnerAsync(commerce.UserId);
                if (commerceUser != null)
                {
                    await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                    {
                        To = commerceUser.Email,
                        Subject = $"Pago recibido a trav\u00e9s de tarjeta {lastFour}",
                        HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#16a34a,#22c55e);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128176; Pago Recibido - Hermes Pay</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{commerce.Name}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Ha recibido un nuevo pago mediante Hermes Pay.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Tarjeta</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFour}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto recibido</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#16a34a;">RD${command.TransactionAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha y hora</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
<p style="color:#64748b;font-size:13px;margin:0;">Este mensaje sirve como constancia del pago recibido.</p>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Hermes Pay</p>
</div>
</div>
</body></html>
"""
                    });
                }
            }
            catch { }
        }

        private async Task<ABP.Core.Application.Dtos.User.UserDto?> GetCardOwnerAsync(string clientId)
        {
            try
            {
                return await _accountService.GetUserById(clientId);
            }
            catch { return null; }
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
