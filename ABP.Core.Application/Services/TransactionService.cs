using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class TransactionService : GenericService<Transaction, TransactionDto>, ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly IEmailService _emailService;
        private readonly IBaseAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ISavingAccountRepository savingAccountRepository,
            ICreditCardRepository creditCardRepository,
            ICardTransactionRepository cardTransactionRepository,
            ILoanRepository loanRepository,
            ILoanInstallmentRepository loanInstallmentRepository,
            IEmailService emailService,
            IBaseAccountService accountService,
            IMapper mapper,
            ILoggerFactory loggerFactory) : base(transactionRepository, mapper, loggerFactory.CreateLogger<GenericService<Transaction, TransactionDto>>())
        {
            _transactionRepository = transactionRepository;
            _savingAccountRepository = savingAccountRepository;
            _creditCardRepository = creditCardRepository;
            _cardTransactionRepository = cardTransactionRepository;
            _loanRepository = loanRepository;
            _loanInstallmentRepository = loanInstallmentRepository;
            _emailService = emailService;
            _accountService = accountService;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<TransactionService>();
        }

        private List<string> _includes => new() { "SavingAccount" };

        public override async Task<TransactionDto?> GetByIdAsync(int id)
        {
            var list = await _transactionRepository.GetAllListWithInclude(_includes);
            var entity = list.FirstOrDefault(t => t.Id == id);
            return entity == null ? null : _mapper.Map<TransactionDto>(entity);
        }

        public override async Task<List<TransactionDto>> GetAllAsync()
        {
            var list = await _transactionRepository.GetAllListWithInclude(_includes);
            return _mapper.Map<List<TransactionDto>>(list);
        }

        public async Task<List<TransactionDto>> GetTransactionsByAccountIdAsync(int accountId)
        {
            var allTransactions = await _transactionRepository.GetAllListWithInclude(_includes);
            var accountTransactions = allTransactions
                .Where(t => t.SavingAccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            return _mapper.Map<List<TransactionDto>>(accountTransactions);
        }

        public async Task<bool> TransferAsync(SaveTransferDto dto)
        {
            _logger.LogInformation("Initiating transfer of RD${Amount} from account {Origin} to account {Destination}", dto.Amount, dto.OriginAccountNumber, dto.DestinationAccountNumber);
            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var originAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.OriginAccountNumber);
            var destinationAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.DestinationAccountNumber);

            if (originAccount == null || destinationAccount == null)
            {
                _logger.LogWarning("Transfer failed: Origin or destination account not found.");
                return false;
            }

            // Prevent self-transfer
            if (originAccount.Id == destinationAccount.Id)
            {
                _logger.LogWarning("Transfer failed: Cannot transfer to the same account.");
                return false;
            }

            // Prevent transfer to canceled accounts
            if (destinationAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Transfer failed: Destination account is not active.");
                return false;
            }

            if (originAccount.Balance < dto.Amount)
            {
                _logger.LogWarning("Transfer failed: Insufficient funds in origin account {Origin}", dto.OriginAccountNumber);
                // Register rejected transaction
                var rejectedTx = new Transaction
                {
                    SavingAccountId = originAccount.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = dto.Amount,
                    Type = TransactionType.Debit,
                    Beneficiary = destinationAccount.AccountNumber,
                    Origin = originAccount.AccountNumber,
                    Status = TransactionStatus.Rejected
                };
                await _transactionRepository.AddAsync(rejectedTx);
                return false;
            }

            originAccount.Balance -= dto.Amount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            destinationAccount.Balance += dto.Amount;
            await _savingAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            var debitTransaction = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Debit,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(debitTransaction);

            var creditTransaction = new Transaction
            {
                SavingAccountId = destinationAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Credit,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(creditTransaction);

            _logger.LogInformation("Transfer completed successfully.");

            // Send email to origin account holder
            try
            {
                var originOwner = await _accountService.GetUserById(originAccount.ClientId);
                if (originOwner != null)
                {
                    var lastFourDest = destinationAccount.AccountNumber.Substring(destinationAccount.AccountNumber.Length - 4);
                    var lastFourOrig = originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4);
                    await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                    {
                        To = originOwner.Email,
                        Subject = $"Transferencia realizada a cuenta {lastFourDest}",
                        HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#0891b2,#22d3ee);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128260; Transferencia Realizada</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{originOwner.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha procesado una transferencia exitosa desde tu cuenta.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Monto transferido</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#0891b2;border-radius:0 6px 6px 0;">RD${dto.Amount:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Cuenta origen</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">&#9679;&#9679;&#9679;&#9679; {lastFourOrig}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Cuenta destino</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; {lastFourDest}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Nuevo balance</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">RD${originAccount.Balance:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha y hora</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
"""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Transfer completed but email notification failed.");
            }

            return true;
        }

        public async Task<bool> CashAdvanceAsync(SaveCashAdvanceDto dto)
        {
            _logger.LogInformation("Initiating cash advance of RD${Amount} from credit card {Origin} to account {Destination}", dto.Amount, dto.OriginCreditCardNumber, dto.DestinationAccountNumber);
            var allCreditCards = await _creditCardRepository.GetAllListAsync();
            var creditCard = allCreditCards.FirstOrDefault(c => c.CardNumber == dto.OriginCreditCardNumber);

            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var destinationAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.DestinationAccountNumber);

            if (creditCard == null || destinationAccount == null)
            {
                _logger.LogWarning("Cash advance failed: Origin credit card or destination account not found.");
                return false;
            }

            var availableLimit = creditCard.CreditLimit - creditCard.CurrentDebt;
            var amountWithInterest = dto.Amount + (dto.Amount * 0.0625m); // 6.25% de interes

            if (availableLimit < amountWithInterest)
            {
                _logger.LogWarning("Cash advance failed: Insufficient credit limit in credit card {Origin}", dto.OriginCreditCardNumber);
                // Register rejected card transaction
                var rejectedCardTx = new CardTransaction
                {
                    CreditCardId = creditCard.Id,
                    TransactionDate = DateTime.UtcNow,
                    Amount = amountWithInterest,
                    CommerceName = "Avance de Efectivo - Rechazado",
                    Status = TransactionStatus.Rejected
                };
                await _cardTransactionRepository.AddAsync(rejectedCardTx);
                return false;
            }

            creditCard.CurrentDebt += amountWithInterest;
            await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

            destinationAccount.Balance += dto.Amount;
            await _savingAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            var cardTransaction = new CardTransaction
            {
                CreditCardId = creditCard.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = amountWithInterest, 
                CommerceName = "Avance de Efectivo",
                Status = TransactionStatus.Approved
            };
            await _cardTransactionRepository.AddAsync(cardTransaction);

            var creditTransaction = new Transaction
            {
                SavingAccountId = destinationAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.CashAdvance,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = creditCard.CardNumber.Substring(creditCard.CardNumber.Length - 4),
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(creditTransaction);

            _logger.LogInformation("Cash advance completed successfully.");

            // Send email to card holder
            try
            {
                var cardOwner = await _accountService.GetUserById(creditCard.ClientId);
                if (cardOwner != null)
                {
                    var lastFourCard = creditCard.CardNumber.Substring(creditCard.CardNumber.Length - 4);
                    var lastFourAcc = destinationAccount.AccountNumber.Substring(destinationAccount.AccountNumber.Length - 4);
                    await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                    {
                        To = cardOwner.Email,
                        Subject = $"Avance de efectivo por RD${dto.Amount:N2}",
                        HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#16a34a,#22c55e);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128176; Avance de Efectivo</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{cardOwner.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha procesado un avance de efectivo desde tu tarjeta de cr&#233;dito.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Monto del avance</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#16a34a;border-radius:0 6px 6px 0;">RD${dto.Amount:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Comisi&#243;n (6.25%)</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#dc2626;">RD${dto.Amount * 0.0625m:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Total debitado</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">RD${amountWithInterest:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Tarjeta</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFourCard}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Cuenta destino</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; {lastFourAcc}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha y hora</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
"""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cash advance completed but email notification failed.");
            }

            return true;
        }

        public async Task<bool> CreditCardPaymentAsync(SaveCreditCardPaymentDto dto)
        {
            _logger.LogInformation("Initiating credit card payment of RD${Amount} to card {Card}", dto.Amount, dto.CreditCardNumber);
            var allCreditCards = await _creditCardRepository.GetAllListAsync();
            var creditCard = allCreditCards.FirstOrDefault(c => c.CardNumber == dto.CreditCardNumber);

            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var originAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.OriginAccountNumber);

            if (creditCard == null || originAccount == null)
            {
                _logger.LogWarning("Credit card payment failed: Card or account not found.");
                return false;
            }

            if (creditCard.Status != CreditCardStatus.Active)
            {
                _logger.LogWarning("Credit card payment failed: Card is inactive.");
                return false;
            }

            if (originAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Credit card payment failed: Origin account is inactive.");
                return false;
            }

            if (creditCard.CurrentDebt <= 0)
            {
                _logger.LogWarning("Credit card payment failed: Card has no debt.");
                return false;
            }

            // Cap payment to actual debt (no sobrepago)
            var effectiveAmount = Math.Min(dto.Amount, creditCard.CurrentDebt);

            if (originAccount.Balance < effectiveAmount)
            {
                _logger.LogWarning("Credit card payment failed: Insufficient funds in origin account.");
                return false;
            }

            originAccount.Balance -= effectiveAmount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            creditCard.CurrentDebt -= effectiveAmount;
            await _creditCardRepository.UpdateAsync(creditCard.Id, creditCard);

            // Register card transaction
            var cardTransaction = new CardTransaction
            {
                CreditCardId = creditCard.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = effectiveAmount,
                CommerceName = "Pago en Cuenta",
                Status = TransactionStatus.Approved
            };
            await _cardTransactionRepository.AddAsync(cardTransaction);

            // Register debit in savings account
            var debitTransaction = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = effectiveAmount,
                Type = TransactionType.Debit,
                Beneficiary = creditCard.CardNumber.Substring(creditCard.CardNumber.Length - 4),
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(debitTransaction);

            _logger.LogInformation("Credit card payment completed successfully.");

            // Send email notification to the card owner
            try
            {
                var cardOwner = await _accountService.GetUserById(creditCard.ClientId);
                if (cardOwner != null)
                {
                    var lastFourCard = creditCard.CardNumber.Substring(creditCard.CardNumber.Length - 4);
                    var lastFourAccount = originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4);
                    await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                    {
                        To = cardOwner.Email,
                        Subject = $"Pago realizado a la tarjeta {lastFourCard}",
                        HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#1a56db,#3b82f6);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128179; Pago a Tarjeta</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{cardOwner.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha procesado un pago a su tarjeta de cr&#233;dito.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Monto pagado</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#16a34a;border-radius:0 6px 6px 0;">RD${effectiveAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Tarjeta</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">&#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFourCard}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Cuenta origen</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; {lastFourAccount}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha y hora</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
"""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Credit card payment completed but email notification failed.");
            }

            return true;
        }

        public async Task<bool> LoanPaymentAsync(SaveLoanPaymentDto dto)
        {
            _logger.LogInformation("Initiating loan payment of RD${Amount} to loan {Loan}", dto.Amount, dto.LoanNumber);
            var allLoans = await _loanRepository.GetAllListAsync();
            var loan = allLoans.FirstOrDefault(l => l.LoanNumber == dto.LoanNumber);

            var allAccounts = await _savingAccountRepository.GetAllListAsync();
            var originAccount = allAccounts.FirstOrDefault(a => a.AccountNumber == dto.OriginAccountNumber);

            if (loan == null || originAccount == null)
            {
                _logger.LogWarning("Loan payment failed: Loan or account not found.");
                return false;
            }

            if (loan.Status != LoanStatus.Active)
            {
                _logger.LogWarning("Loan payment failed: Loan is not active.");
                return false;
            }

            if (originAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Loan payment failed: Origin account is inactive.");
                return false;
            }

            if (loan.AmountPending <= 0)
            {
                _logger.LogWarning("Loan payment failed: Loan has no pending amount.");
                return false;
            }

            // Cap payment to actual pending amount
            var effectiveAmount = Math.Min(dto.Amount, loan.AmountPending);

            if (originAccount.Balance < effectiveAmount)
            {
                _logger.LogWarning("Loan payment failed: Insufficient funds in origin account.");
                return false;
            }

            originAccount.Balance -= effectiveAmount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            loan.AmountPending -= effectiveAmount;
            loan.PaidInstallments++;
            if (loan.AmountPending <= 0)
            {
                loan.AmountPending = 0;
                loan.Status = LoanStatus.Completed;
            }
            await _loanRepository.UpdateAsync(loan.Id, loan);

            // Apply to oldest pending installment first
            var installments = await _loanInstallmentRepository.GetAllListAsync();
            var pendingInstallments = installments
                .Where(i => i.LoanId == loan.Id && i.PaymentStatus != PaymentStatus.Paid)
                .OrderBy(i => i.DueDate)
                .ToList();

            decimal remaining = effectiveAmount;
            foreach (var inst in pendingInstallments)
            {
                if (remaining <= 0) break;
                var payment = Math.Min(remaining, inst.PendingAmount);
                inst.PendingAmount -= payment;
                remaining -= payment;
                if (inst.PendingAmount <= 0)
                {
                    inst.PaymentStatus = PaymentStatus.Paid;
                    inst.IsLate = false;
                }
                else
                {
                    inst.PaymentStatus = PaymentStatus.PartiallyPaid;
                }
                await _loanInstallmentRepository.UpdateAsync(inst.Id, inst);
            }

            // Register debit in savings account
            var debitTransaction = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = effectiveAmount,
                Type = TransactionType.LoanPayment,
                Beneficiary = loan.LoanNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(debitTransaction);

            _logger.LogInformation("Loan payment completed successfully.");

            // Send email notification to the client
            try
            {
                var loanClient = await _accountService.GetUserById(loan.ClientId);
                if (loanClient != null)
                {
                    var lastFourAccount = originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4);
                    await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                    {
                        To = loanClient.Email,
                        Subject = $"Pago realizado al pr&#233;stamo {loan.LoanNumber}",
                        HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#16a34a,#22c55e);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128176; Pago a Pr&#233;stamo</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{loanClient.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se ha procesado un pago a su pr&#233;stamo.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Monto pagado</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#16a34a;border-radius:0 6px 6px 0;">RD${effectiveAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">N&#250;mero de pr&#233;stamo</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">#{loan.LoanNumber}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Cuenta origen</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; {lastFourAccount}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha y hora</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
"""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Loan payment completed but email notification failed.");
            }

            return true;
        }
    }
}
