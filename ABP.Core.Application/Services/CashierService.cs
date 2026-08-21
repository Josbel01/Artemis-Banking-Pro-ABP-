using ABP.Core.Application.Dtos.Cashier;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ABP.Core.Application.Services
{
    public class CashierService : ICashierService
    {
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly IEmailService _emailService;
        private readonly IBaseAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<CashierService> _logger;

        public CashierService(
            ISavingAccountRepository savingAccountRepository,
            ICreditCardRepository creditCardRepository,
            ILoanRepository loanRepository,
            ITransactionRepository transactionRepository,
            ICardTransactionRepository cardTransactionRepository,
            IEmailService emailService,
            IBaseAccountService accountService,
            IMapper mapper,
            ILogger<CashierService> logger)
        {
            _savingAccountRepository = savingAccountRepository;
            _creditCardRepository = creditCardRepository;
            _loanRepository = loanRepository;
            _transactionRepository = transactionRepository;
            _cardTransactionRepository = cardTransactionRepository;
            _emailService = emailService;
            _accountService = accountService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OperationResultDto> DepositAsync(CashierDepositDto dto)
        {
            _logger.LogInformation("Cashier {CashierId} initiating deposit of RD${Amount} to account {Account}", dto.ResponsibleUserId, dto.Amount, dto.AccountNumber);
            var account = await _savingAccountRepository.GetByAccountNumberAsync(dto.AccountNumber);

            if (account == null)
            {
                _logger.LogWarning("Deposit failed: Account {Account} not found.", dto.AccountNumber);
                return Error("No se encontró una cuenta con el número indicado.");
            }

            if (account.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Deposit failed: Account {Account} is inactive.", dto.AccountNumber);
                return Error("La cuenta está inactiva y no puede recibir depósitos.");
            }

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Deposit failed: Invalid amount {Amount}.", dto.Amount);
                return Error("El monto a depositar debe ser mayor a cero.");
            }

            account.Balance += dto.Amount;
            await _savingAccountRepository.UpdateAsync(account.Id, account);

            var transaction = new Transaction
            {
                SavingAccountId = account.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Deposit,
                Beneficiary = account.AccountNumber,
                Origin = "Cajero",
                Status = TransactionStatus.Approved,
                ResponsibleUserId = dto.ResponsibleUserId
            };
            var saved = await _transactionRepository.AddAsync(transaction);

            _logger.LogInformation("Deposit of RD${Amount} to account {Account} completed successfully. TxId: {TxId}", dto.Amount, dto.AccountNumber, saved?.Id);

            // Get account holder name
            var accountOwner = await _accountService.GetUserById(account.ClientId);
            var holderName = accountOwner != null ? $"{accountOwner.FirstName} {accountOwner.LastName}" : "";
            var lastFourAccount = account.AccountNumber.Length >= 4 ? account.AccountNumber.Substring(account.AccountNumber.Length - 4) : account.AccountNumber;

            var result = new OperationResultDto
            {
                Success = true,
                OperationType = "Depósito",
                Amount = dto.Amount,
                AccountNumber = account.AccountNumber,
                AccountHolderName = holderName,
                NewBalance = account.Balance,
                OperationDate = DateTime.Now,
                TransactionId = saved?.Id ?? 0
            };

            // Send email to account holder
            await SendAccountEmailAsync(account.ClientId,
                "Depósito recibido en tu cuenta",
                "💰 Depósito",
                $"Se ha realizado un depósito exitoso en tu cuenta de ahorro.",
                new[] {
                    ("Monto depositado", $"RD${dto.Amount:N2}"),
                    ("Cuenta destino", $"****{lastFourAccount}"),
                    ("Nuevo balance", $"RD${account.Balance:N2}"),
                    ("Fecha y hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                },
                "#16a34a", "#22c55e");

            return result;
        }

        public async Task<OperationResultDto> WithdrawalAsync(CashierWithdrawalDto dto)
        {
            _logger.LogInformation("Cashier {CashierId} initiating withdrawal of RD${Amount} from account {Account}", dto.ResponsibleUserId, dto.Amount, dto.AccountNumber);
            var account = await _savingAccountRepository.GetByAccountNumberAsync(dto.AccountNumber);

            if (account == null)
            {
                _logger.LogWarning("Withdrawal failed: Account {Account} not found.", dto.AccountNumber);
                return Error("No se encontró una cuenta con el número indicado.");
            }

            if (account.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Withdrawal failed: Account {Account} is inactive.", dto.AccountNumber);
                return Error("La cuenta está inactiva y no puede procesar retiros.");
            }

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Withdrawal failed: Invalid amount {Amount}.", dto.Amount);
                return Error("El monto a retirar debe ser mayor a cero.");
            }

            if (account.Balance < dto.Amount)
            {
                _logger.LogWarning("Withdrawal failed: Insufficient funds in account {Account}.", dto.AccountNumber);
                return Error($"Fondos insuficientes. Balance disponible: RD${account.Balance:N2}");
            }

            account.Balance -= dto.Amount;
            await _savingAccountRepository.UpdateAsync(account.Id, account);

            var transaction = new Transaction
            {
                SavingAccountId = account.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Withdrawal,
                Beneficiary = "Titular",
                Origin = account.AccountNumber,
                Status = TransactionStatus.Approved,
                ResponsibleUserId = dto.ResponsibleUserId
            };
            var saved = await _transactionRepository.AddAsync(transaction);

            _logger.LogInformation("Withdrawal of RD${Amount} from account {Account} completed successfully. TxId: {TxId}", dto.Amount, dto.AccountNumber, saved?.Id);

            // Get account holder name
            var accountOwner = await _accountService.GetUserById(account.ClientId);
            var holderName = accountOwner != null ? $"{accountOwner.FirstName} {accountOwner.LastName}" : "";
            var lastFourAccount = account.AccountNumber.Length >= 4 ? account.AccountNumber.Substring(account.AccountNumber.Length - 4) : account.AccountNumber;

            var result = new OperationResultDto
            {
                Success = true,
                OperationType = "Retiro",
                Amount = dto.Amount,
                AccountNumber = account.AccountNumber,
                AccountHolderName = holderName,
                NewBalance = account.Balance,
                OperationDate = DateTime.Now,
                TransactionId = saved?.Id ?? 0
            };

            // Send email to account holder
            await SendAccountEmailAsync(account.ClientId,
                "Retiro realizado en tu cuenta",
                "🏧 Retiro",
                $"Se ha realizado un retiro exitoso de tu cuenta de ahorro.",
                new[] {
                    ("Monto retirado", $"RD${dto.Amount:N2}"),
                    ("Cuenta origen", $"****{lastFourAccount}"),
                    ("Nuevo balance", $"RD${account.Balance:N2}"),
                    ("Fecha y hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                },
                "#dc2626", "#ef4444");

            return result;
        }

        public async Task<OperationResultDto> CreditCardPaymentAsync(CashierCreditCardPaymentDto dto)
        {
            _logger.LogInformation("Cashier {CashierId} initiating credit card payment of RD${Amount} to card {Card}", dto.ResponsibleUserId, dto.Amount, dto.CardNumber);
            var card = await _creditCardRepository.GetByCardNumberAsync(dto.CardNumber);

            if (card == null)
            {
                _logger.LogWarning("Credit card payment failed: Card {Card} not found.", dto.CardNumber);
                return Error("No se encontró una tarjeta de crédito con el número indicado.");
            }

            if (card.Status != CreditCardStatus.Active)
            {
                _logger.LogWarning("Credit card payment failed: Card {Card} is inactive.", dto.CardNumber);
                return Error("La tarjeta de crédito está inactiva.");
            }

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Credit card payment failed: Invalid amount {Amount}.", dto.Amount);
                return Error("El monto del pago debe ser mayor a cero.");
            }

            if (dto.Amount > card.CurrentDebt)
            {
                _logger.LogWarning("Credit card payment failed: Amount {Amount} exceeds current debt of {Debt}.", dto.Amount, card.CurrentDebt);
                return Error($"El monto supera la deuda actual (RD${card.CurrentDebt:N2}). Use un monto igual o menor.");
            }

            // Debit from origin savings account
            var originAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.OriginAccountNumber);
            if (originAccount == null)
                return Error("No se encontró la cuenta de origen.");
            if (originAccount.Status != SavingAccountStatus.Active)
                return Error("La cuenta de origen está inactiva.");
            if (originAccount.Balance < dto.Amount)
                return Error($"Fondos insuficientes. Balance disponible: RD${originAccount.Balance:N2}");

            originAccount.Balance -= dto.Amount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            card.CurrentDebt -= dto.Amount;
            await _creditCardRepository.UpdateAsync(card.Id, card);

            var cardTransaction = new CardTransaction
            {
                CreditCardId = card.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                CommerceName = "Pago en Caja",
                Status = TransactionStatus.Approved
            };
            var savedCard = await _cardTransactionRepository.AddAsync(cardTransaction);

            // Register debit in savings account
            var debitTx = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.CreditCardPayment,
                Beneficiary = card.CardNumber.Substring(card.CardNumber.Length - 4),
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved,
                ResponsibleUserId = dto.ResponsibleUserId
            };
            var saved = await _transactionRepository.AddAsync(debitTx);

            _logger.LogInformation("Credit card payment of RD${Amount} to card {Card} completed successfully. TxId: {TxId}", dto.Amount, dto.CardNumber, saved?.Id);

            var lastFourCard = card.CardNumber.Substring(card.CardNumber.Length - 4);

            var result = new OperationResultDto
            {
                Success = true,
                OperationType = "Pago a Tarjeta de Crédito",
                Amount = dto.Amount,
                AccountNumber = originAccount.AccountNumber,
                AccountHolderName = (await _accountService.GetUserById(originAccount.ClientId)) != null ? $"{(await _accountService.GetUserById(originAccount.ClientId))!.FirstName} {(await _accountService.GetUserById(originAccount.ClientId))!.LastName}" : "",
                NewBalance = card.CurrentDebt,
                OperationDate = DateTime.Now,
                TransactionId = saved?.Id ?? 0
            };

            // Send email to card holder
            await SendAccountEmailAsync(card.ClientId,
                $"Pago realizado a la tarjeta {lastFourCard}",
                "💳 Pago a Tarjeta",
                $"Se ha procesado un pago a tu tarjeta de crédito en caja.",
                new[] {
                    ("Monto pagado", $"RD${dto.Amount:N2}"),
                    ("Tarjeta", $"•••• •••• •••• {lastFourCard}"),
                    ("Deuda restante", $"RD${card.CurrentDebt:N2}"),
                    ("Fecha y hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                },
                "#1a56db", "#3b82f6");

            // Send email to origin account owner if different from card owner
            if (originAccount.ClientId != card.ClientId)
            {
                var lastFourOrigin = originAccount.AccountNumber.Length >= 4
                    ? originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4)
                    : originAccount.AccountNumber;

                await SendAccountEmailAsync(originAccount.ClientId,
                    $"Pago a tarjeta desde tu cuenta",
                    "💳 Débito por Pago a Tarjeta",
                    $"Se ha debitado dinero de tu cuenta de ahorro para realizar un pago a una tarjeta de crédito.",
                    new[] {
                        ("Monto debitado", $"RD${dto.Amount:N2}"),
                        ("Cuenta origen", $"****{lastFourOrigin}"),
                        ("Tarjeta pagada", $"•••• •••• •••• {lastFourCard}"),
                        ("Nuevo balance", $"RD${originAccount.Balance:N2}"),
                        ("Fecha y hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    },
                    "#dc2626", "#ef4444");
            }

            return result;
        }

        public async Task<OperationResultDto> LoanPaymentAsync(CashierLoanPaymentDto dto)
        {
            _logger.LogInformation("Cashier {CashierId} initiating loan payment of RD${Amount} to loan {Loan}", dto.ResponsibleUserId, dto.Amount, dto.LoanNumber);
            var loan = await _loanRepository.GetByLoanNumberAsync(dto.LoanNumber);

            if (loan == null)
            {
                _logger.LogWarning("Loan payment failed: Loan {Loan} not found.", dto.LoanNumber);
                return Error("No se encontró un préstamo con el número indicado.");
            }

            if (loan.Status != LoanStatus.Active)
            {
                _logger.LogWarning("Loan payment failed: Loan {Loan} is not active.", dto.LoanNumber);
                return Error("El préstamo no está activo.");
            }

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Loan payment failed: Invalid amount {Amount}.", dto.Amount);
                return Error("El monto del pago debe ser mayor a cero.");
            }

            if (dto.Amount > loan.AmountPending)
            {
                _logger.LogWarning("Loan payment failed: Amount {Amount} exceeds pending amount of {Pending}.", dto.Amount, loan.AmountPending);
                return Error($"El monto supera el saldo pendiente (RD${loan.AmountPending:N2}).");
            }

            // Debit from origin savings account
            var originAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.OriginAccountNumber);
            if (originAccount == null)
                return Error("No se encontró la cuenta de origen.");
            if (originAccount.Status != SavingAccountStatus.Active)
                return Error("La cuenta de origen está inactiva.");
            if (originAccount.Balance < dto.Amount)
                return Error($"Fondos insuficientes. Balance disponible: RD${originAccount.Balance:N2}");

            originAccount.Balance -= dto.Amount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            loan.AmountPending -= dto.Amount;
            loan.PaidInstallments++;

            if (loan.AmountPending <= 0)
            {
                loan.AmountPending = 0;
                loan.Status = LoanStatus.Completed;
            }

            await _loanRepository.UpdateAsync(loan.Id, loan);

            // Register debit in savings account
            var debitTx = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.LoanPayment,
                Beneficiary = loan.LoanNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved,
                ResponsibleUserId = dto.ResponsibleUserId
            };
            var saved = await _transactionRepository.AddAsync(debitTx);

            _logger.LogInformation("Loan payment of RD${Amount} to loan {Loan} completed successfully.", dto.Amount, dto.LoanNumber);

            var result = new OperationResultDto
            {
                Success = true,
                OperationType = "Pago a Préstamo",
                Amount = dto.Amount,
                AccountNumber = originAccount.AccountNumber,
                AccountHolderName = (await _accountService.GetUserById(originAccount.ClientId)) != null ? $"{(await _accountService.GetUserById(originAccount.ClientId))!.FirstName} {(await _accountService.GetUserById(originAccount.ClientId))!.LastName}" : "",
                NewBalance = loan.AmountPending,
                OperationDate = DateTime.Now,
                TransactionId = saved?.Id ?? 0
            };

            // Send email to loan client
            await SendAccountEmailAsync(loan.ClientId,
                $"Pago realizado al préstamo {loan.LoanNumber}",
                "💰 Pago a Préstamo",
                $"Se ha procesado un pago a tu préstamo en caja.",
                new[] {
                    ("Monto pagado", $"RD${dto.Amount:N2}"),
                    ("Préstamo", $"#{loan.LoanNumber}"),
                    ("Pendiente restante", $"RD${loan.AmountPending:N2}"),
                    ("Cuotas pagadas", $"{loan.PaidInstallments}"),
                    ("Fecha y hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                },
                "#16a34a", "#22c55e");

            return result;
        }

        public async Task<OperationResultDto> TransferBetweenAccountsAsync(CashierTransferDto dto)
        {
            _logger.LogInformation("Cashier {CashierId} initiating transfer of RD${Amount} from {Origin} to {Destination}", dto.ResponsibleUserId, dto.Amount, dto.OriginAccountNumber, dto.DestinationAccountNumber);
            if (dto.OriginAccountNumber == dto.DestinationAccountNumber)
            {
                _logger.LogWarning("Transfer failed: Origin and destination accounts are the same.");
                return Error("La cuenta de origen y destino no pueden ser la misma.");
            }

            var originAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.OriginAccountNumber);
            var destinationAccount = await _savingAccountRepository.GetByAccountNumberAsync(dto.DestinationAccountNumber);

            if (originAccount == null)
            {
                _logger.LogWarning("Transfer failed: Origin account {Origin} not found.", dto.OriginAccountNumber);
                return Error("No se encontró la cuenta de origen.");
            }

            if (destinationAccount == null)
            {
                _logger.LogWarning("Transfer failed: Destination account {Destination} not found.", dto.DestinationAccountNumber);
                return Error("No se encontró la cuenta de destino.");
            }

            if (originAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Transfer failed: Origin account {Origin} is inactive.", dto.OriginAccountNumber);
                return Error("La cuenta de origen está inactiva.");
            }

            if (destinationAccount.Status != SavingAccountStatus.Active)
            {
                _logger.LogWarning("Transfer failed: Destination account {Destination} is inactive.", dto.DestinationAccountNumber);
                return Error("La cuenta de destino está inactiva.");
            }

            if (dto.Amount <= 0)
            {
                _logger.LogWarning("Transfer failed: Invalid amount {Amount}.", dto.Amount);
                return Error("El monto de la transferencia debe ser mayor a cero.");
            }

            if (originAccount.Balance < dto.Amount)
            {
                _logger.LogWarning("Transfer failed: Insufficient funds in origin account {Origin}.", dto.OriginAccountNumber);
                return Error($"Fondos insuficientes en la cuenta origen. Balance: RD${originAccount.Balance:N2}");
            }

            originAccount.Balance -= dto.Amount;
            await _savingAccountRepository.UpdateAsync(originAccount.Id, originAccount);

            destinationAccount.Balance += dto.Amount;
            await _savingAccountRepository.UpdateAsync(destinationAccount.Id, destinationAccount);

            var debitTx = new Transaction
            {
                SavingAccountId = originAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Transfer,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved,
                ResponsibleUserId = dto.ResponsibleUserId
            };
            var savedDebit = await _transactionRepository.AddAsync(debitTx);

            var creditTx = new Transaction
            {
                SavingAccountId = destinationAccount.Id,
                TransactionDate = DateTime.UtcNow,
                Amount = dto.Amount,
                Type = TransactionType.Transfer,
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = TransactionStatus.Approved
            };
            await _transactionRepository.AddAsync(creditTx);

            _logger.LogInformation("Transfer of RD${Amount} completed successfully. Debit TxId: {TxId}", dto.Amount, savedDebit?.Id);

            // Get account holder names
            var originOwner = await _accountService.GetUserById(originAccount.ClientId);
            var destOwner = await _accountService.GetUserById(destinationAccount.ClientId);
            var originHolderName = originOwner != null ? $"{originOwner.FirstName} {originOwner.LastName}" : "";
            var destHolderName = destOwner != null ? $"{destOwner.FirstName} {destOwner.LastName}" : "";
            var lastFourOrigin = originAccount.AccountNumber.Length >= 4 ? originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4) : originAccount.AccountNumber;
            var lastFourDest = destinationAccount.AccountNumber.Length >= 4 ? destinationAccount.AccountNumber.Substring(destinationAccount.AccountNumber.Length - 4) : destinationAccount.AccountNumber;

            var result = new OperationResultDto
            {
                Success = true,
                OperationType = "Transferencia entre Cuentas",
                Amount = dto.Amount,
                AccountNumber = originAccount.AccountNumber,
                DestinationAccountNumber = destinationAccount.AccountNumber,
                AccountHolderName = originHolderName,
                DestinationHolderName = destHolderName,
                NewBalance = originAccount.Balance,
                OperationDate = DateTime.Now,
                TransactionId = savedDebit?.Id ?? 0
            };

            // Send email to origin account holder
            await SendAccountEmailAsync(originAccount.ClientId,
                "Transferencia realizada desde tu cuenta",
                "🔄 Transferencia",
                $"Se ha realizado una transferencia exitosa desde tu cuenta.",
                new[] {
                    ("Monto transferido", $"RD${dto.Amount:N2}"),
                    ("Cuenta origen", $"****{lastFourOrigin}"),
                    ("Cuenta destino", $"****{lastFourDest}"),
                    ("Nuevo balance", $"RD${originAccount.Balance:N2}"),
                    ("Fecha y hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                },
                "#0891b2", "#22d3ee");

            // Send email to destination account holder
            await SendAccountEmailAsync(destinationAccount.ClientId,
                "Transferencia recibida en tu cuenta",
                "💰 Transferencia Recibida",
                $"Se ha recibido una transferencia en tu cuenta de ahorro.",
                new[] {
                    ("Monto recibido", $"RD${dto.Amount:N2}"),
                    ("Cuenta destino", $"****{lastFourDest}"),
                    ("Cuenta origen", $"****{lastFourOrigin}"),
                    ("Nuevo balance", $"RD${destinationAccount.Balance:N2}"),
                    ("Fecha y hora", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                },
                "#16a34a", "#22c55e");

            return result;
        }


        public async Task<DailyIndicatorsDto> GetDailyIndicatorsAsync(string cashierUserId)
        {
            var allTransactions = await _transactionRepository.GetAllListAsync();
            var today = DateTime.UtcNow.Date;

            var dailyByMe = allTransactions
                .Where(t => t.ResponsibleUserId == cashierUserId && t.TransactionDate.Date == today)
                .ToList();

            return new DailyIndicatorsDto
            {
                TotalDeposits        = dailyByMe.Count(t => t.Type == TransactionType.Deposit),
                TotalWithdrawals     = dailyByMe.Count(t => t.Type == TransactionType.Withdrawal),
                TotalCreditCardPayments = dailyByMe.Count(t => t.Type == TransactionType.CreditCardPayment),
                TotalLoanPayments    = dailyByMe.Count(t => t.Type == TransactionType.LoanPayment),
                TotalTransfers       = dailyByMe.Count(t => t.Type == TransactionType.Transfer),
                TotalAmountOperated  = dailyByMe.Sum(t => t.Amount)
            };
        }

        public async Task<List<TransactionDto>> GetDailyTransactionsByCashierAsync(string cashierUserId)
        {
            var allTransactions = await _transactionRepository.GetAllListAsync();
            var today = DateTime.UtcNow.Date;

            var daily = allTransactions
                .Where(t => t.ResponsibleUserId == cashierUserId && t.TransactionDate.Date == today)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            return _mapper.Map<List<TransactionDto>>(daily);
        }

        // ─── Email helper ──────────────────────────────────────
        private async Task SendAccountEmailAsync(string userId, string subject, string icon, string description, (string label, string value)[] rows, string colorDark, string colorLight)
        {
            try
            {
                var user = await _accountService.GetUserById(userId);
                if (user == null) return;

                string rowsHtml = "";
                for (int i = 0; i < rows.Length; i++)
                {
                    string bgColor = i % 2 == 0 ? "#f8fafc" : "#ffffff";
                    string radius = i == 0 ? "border-radius:6px 0 0 6px;" : i == rows.Length - 1 ? "border-radius:0 6px 6px 0;" : "";
                    rowsHtml += $"<tr><td style=\"padding:10px 14px;background:{bgColor};color:#64748b;font-size:13px;font-weight:600;{radius}\">{rows[i].label}</td><td style=\"padding:10px 14px;background:{bgColor};font-size:15px;font-weight:700;color:#0b1f3a;{radius}\">{rows[i].value}</td></tr>";
                }

                await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                {
                    To = user.Email,
                    Subject = subject,
                    HtmlBody = $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""></head>
<body style=""margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;"">
<div style=""max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);"">
<div style=""background:linear-gradient(135deg,{colorDark},{colorLight});padding:28px 30px;text-align:center;"">
<h1 style=""color:#fff;margin:0;font-size:20px;"">{icon}</h1>
</div>
<div style=""padding:30px;"">
<p style=""color:#334155;font-size:15px;margin:0 0 18px;"">Hola <strong>{user.FirstName}</strong>,</p>
<p style=""color:#334155;font-size:15px;margin:0 0 24px;"">{description}</p>
<table style=""width:100%;border-collapse:collapse;margin-bottom:24px;"">{rowsHtml}</table>
</div>
<div style=""background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;"">
<p style=""color:#94a3b8;font-size:11px;margin:0;"">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>"
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send email for operation to user {UserId}.", userId);
            }
        }

        private static OperationResultDto Error(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}
