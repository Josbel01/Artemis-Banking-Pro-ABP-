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
        private readonly IMapper _mapper;
        private readonly ILogger<CashierService> _logger;

        public CashierService(
            ISavingAccountRepository savingAccountRepository,
            ICreditCardRepository creditCardRepository,
            ILoanRepository loanRepository,
            ITransactionRepository transactionRepository,
            ICardTransactionRepository cardTransactionRepository,
            IMapper mapper,
            ILogger<CashierService> logger)
        {
            _savingAccountRepository = savingAccountRepository;
            _creditCardRepository = creditCardRepository;
            _loanRepository = loanRepository;
            _transactionRepository = transactionRepository;
            _cardTransactionRepository = cardTransactionRepository;
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

            return new OperationResultDto
            {
                Success = true,
                OperationType = "Depósito",
                Amount = dto.Amount,
                AccountNumber = account.AccountNumber,
                NewBalance = account.Balance,
                OperationDate = DateTime.Now,
                TransactionId = saved?.Id ?? 0
            };
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

            return new OperationResultDto
            {
                Success = true,
                OperationType = "Retiro",
                Amount = dto.Amount,
                AccountNumber = account.AccountNumber,
                NewBalance = account.Balance,
                OperationDate = DateTime.Now,
                TransactionId = saved?.Id ?? 0
            };
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
            var saved = await _cardTransactionRepository.AddAsync(cardTransaction);

            _logger.LogInformation("Credit card payment of RD${Amount} to card {Card} completed successfully. TxId: {TxId}", dto.Amount, dto.CardNumber, saved?.Id);

            return new OperationResultDto
            {
                Success = true,
                OperationType = "Pago a Tarjeta de Crédito",
                Amount = dto.Amount,
                AccountNumber = dto.CardNumber,
                NewBalance = card.CurrentDebt,
                OperationDate = DateTime.Now,
                TransactionId = saved?.Id ?? 0
            };
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

            loan.AmountPending -= dto.Amount;
            loan.PaidInstallments++;

            if (loan.AmountPending <= 0)
            {
                loan.AmountPending = 0;
                loan.Status = LoanStatus.Completed;
            }

            await _loanRepository.UpdateAsync(loan.Id, loan);

            _logger.LogInformation("Loan payment of RD${Amount} to loan {Loan} completed successfully.", dto.Amount, dto.LoanNumber);

            return new OperationResultDto
            {
                Success = true,
                OperationType = "Pago a Préstamo",
                Amount = dto.Amount,
                AccountNumber = dto.LoanNumber,
                NewBalance = loan.AmountPending,
                OperationDate = DateTime.Now,
                TransactionId = 0
            };
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
                Status = TransactionStatus.Approved,
                ResponsibleUserId = dto.ResponsibleUserId
            };
            await _transactionRepository.AddAsync(creditTx);

            _logger.LogInformation("Transfer of RD${Amount} completed successfully. Debit TxId: {TxId}", dto.Amount, savedDebit?.Id);

            return new OperationResultDto
            {
                Success = true,
                OperationType = "Transferencia entre Cuentas",
                Amount = dto.Amount,
                AccountNumber = originAccount.AccountNumber,
                DestinationAccountNumber = destinationAccount.AccountNumber,
                NewBalance = originAccount.Balance,
                OperationDate = DateTime.Now,
                TransactionId = savedDebit?.Id ?? 0
            };
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
                TotalCreditCardPayments = 0, // Card transactions are in a different table
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


        private static OperationResultDto Error(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}
