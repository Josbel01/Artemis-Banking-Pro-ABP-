using AutoMapper;
using FluentAssertions;
using ABP.Core.Application.Dtos.Cashier;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Services;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ABP.Unit.Tests.Services
{
    public class CashierServiceTests
    {
        private readonly Mock<ISavingAccountRepository> _savingAccountRepo;
        private readonly Mock<ICreditCardRepository> _creditCardRepo;
        private readonly Mock<ILoanRepository> _loanRepo;
        private readonly Mock<ITransactionRepository> _transactionRepo;
        private readonly Mock<ICardTransactionRepository> _cardTransactionRepo;
        private readonly Mock<IEmailService> _emailService;
        private readonly Mock<IBaseAccountService> _accountService;
        private readonly Mock<IMapper> _mapper;
        private readonly CashierService _service;

        public CashierServiceTests()
        {
            _savingAccountRepo = new Mock<ISavingAccountRepository>();
            _creditCardRepo = new Mock<ICreditCardRepository>();
            _loanRepo = new Mock<ILoanRepository>();
            _transactionRepo = new Mock<ITransactionRepository>();
            _cardTransactionRepo = new Mock<ICardTransactionRepository>();
            _emailService = new Mock<IEmailService>();
            _accountService = new Mock<IBaseAccountService>();
            _mapper = new Mock<IMapper>();
            ILogger<CashierService> logger = new NullLogger<CashierService>();

            _service = new CashierService(
                _savingAccountRepo.Object,
                _creditCardRepo.Object,
                _loanRepo.Object,
                _transactionRepo.Object,
                _cardTransactionRepo.Object,
                _emailService.Object,
                _accountService.Object,
                _mapper.Object,
                logger);
        }

        #region DepositAsync

        [Fact]
        public async Task DepositAsync_Should_Return_Success_When_Valid()
        {
            // Arrange
            var account = new SavingAccount { Id = 1, AccountNumber = "123456789", Balance = 1000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            _savingAccountRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<SavingAccount>())).ReturnsAsync(account);
            _transactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(new Transaction { Id = 1 });

            var dto = new CashierDepositDto { AccountNumber = "123456789", Amount = 500, ResponsibleUserId = "cashier1" };

            // Act
            var result = await _service.DepositAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.OperationType.Should().Be("Depósito");
            result.Amount.Should().Be(500);
            result.NewBalance.Should().Be(1500);
            account.Balance.Should().Be(1500);
        }

        [Fact]
        public async Task DepositAsync_Should_Fail_When_Account_Not_Found()
        {
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("999999999")).ReturnsAsync((SavingAccount?)null);

            var dto = new CashierDepositDto { AccountNumber = "999999999", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.DepositAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("No se encontró");
        }

        [Fact]
        public async Task DepositAsync_Should_Fail_When_Account_Inactive()
        {
            var account = new SavingAccount { Id = 1, AccountNumber = "123456789", Balance = 1000, Status = SavingAccountStatus.Cancelled };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var dto = new CashierDepositDto { AccountNumber = "123456789", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.DepositAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("inactiva");
        }

        [Fact]
        public async Task DepositAsync_Should_Fail_When_Amount_Zero()
        {
            var account = new SavingAccount { Id = 1, AccountNumber = "123456789", Balance = 1000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var dto = new CashierDepositDto { AccountNumber = "123456789", Amount = 0, ResponsibleUserId = "cashier1" };

            var result = await _service.DepositAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("mayor a cero");
        }

        [Fact]
        public async Task DepositAsync_Should_Fail_When_Amount_Negative()
        {
            var account = new SavingAccount { Id = 1, AccountNumber = "123456789", Balance = 1000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);

            var dto = new CashierDepositDto { AccountNumber = "123456789", Amount = -50, ResponsibleUserId = "cashier1" };

            var result = await _service.DepositAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("mayor a cero");
        }

        #endregion

        #region WithdrawalAsync

        [Fact]
        public async Task WithdrawalAsync_Should_Return_Success_When_Valid()
        {
            var account = new SavingAccount { Id = 1, AccountNumber = "123456789", Balance = 5000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123456789")).ReturnsAsync(account);
            _savingAccountRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<SavingAccount>())).ReturnsAsync(account);
            _transactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(new Transaction { Id = 2 });

            var dto = new CashierWithdrawalDto { AccountNumber = "123456789", Amount = 1000, ResponsibleUserId = "cashier1" };

            var result = await _service.WithdrawalAsync(dto);

            result.Success.Should().BeTrue();
            result.OperationType.Should().Be("Retiro");
            result.Amount.Should().Be(1000);
            result.NewBalance.Should().Be(4000);
            account.Balance.Should().Be(4000);
        }

        [Fact]
        public async Task WithdrawalAsync_Should_Fail_When_Account_Not_Found()
        {
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("999")).ReturnsAsync((SavingAccount?)null);

            var dto = new CashierWithdrawalDto { AccountNumber = "999", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.WithdrawalAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("No se encontró");
        }

        [Fact]
        public async Task WithdrawalAsync_Should_Fail_When_Insufficient_Funds()
        {
            var account = new SavingAccount { Id = 1, AccountNumber = "123", Balance = 100, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123")).ReturnsAsync(account);

            var dto = new CashierWithdrawalDto { AccountNumber = "123", Amount = 500, ResponsibleUserId = "cashier1" };

            var result = await _service.WithdrawalAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Fondos insuficientes");
        }

        [Fact]
        public async Task WithdrawalAsync_Should_Fail_When_Account_Inactive()
        {
            var account = new SavingAccount { Id = 1, AccountNumber = "123", Balance = 5000, Status = SavingAccountStatus.Cancelled };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123")).ReturnsAsync(account);

            var dto = new CashierWithdrawalDto { AccountNumber = "123", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.WithdrawalAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("inactiva");
        }

        [Fact]
        public async Task WithdrawalAsync_Should_Fail_When_Amount_Zero()
        {
            var account = new SavingAccount { Id = 1, AccountNumber = "123", Balance = 5000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("123")).ReturnsAsync(account);

            var dto = new CashierWithdrawalDto { AccountNumber = "123", Amount = 0, ResponsibleUserId = "cashier1" };

            var result = await _service.WithdrawalAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("mayor a cero");
        }

        #endregion

        #region CreditCardPaymentAsync

        [Fact]
        public async Task CreditCardPaymentAsync_Should_Return_Success_When_Valid()
        {
            var card = new CreditCard { Id = 1, CardNumber = "4111111111111111", CurrentDebt = 2000, Status = CreditCardStatus.Active, ClientId = "client1" };
            var originAccount = new SavingAccount { Id = 10, AccountNumber = "ACC001", Balance = 5000, Status = SavingAccountStatus.Active, ClientId = "client2" };
            _creditCardRepo.Setup(r => r.GetByCardNumberAsync("4111111111111111")).ReturnsAsync(card);
            _creditCardRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<CreditCard>())).ReturnsAsync(card);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("ACC001")).ReturnsAsync(originAccount);
            _savingAccountRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<SavingAccount>())).ReturnsAsync(originAccount);
            _cardTransactionRepo.Setup(r => r.AddAsync(It.IsAny<CardTransaction>())).ReturnsAsync(new CardTransaction { Id = 1 });
            _transactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(new Transaction { Id = 1 });

            var dto = new CashierCreditCardPaymentDto { CardNumber = "4111111111111111", Amount = 500, OriginAccountNumber = "ACC001", ResponsibleUserId = "cashier1" };

            var result = await _service.CreditCardPaymentAsync(dto);

            result.Success.Should().BeTrue();
            result.OperationType.Should().Be("Pago a Tarjeta de Crédito");
            result.Amount.Should().Be(500);
            result.NewBalance.Should().Be(1500);
            card.CurrentDebt.Should().Be(1500);
            originAccount.Balance.Should().Be(4500);
        }

        [Fact]
        public async Task CreditCardPaymentAsync_Should_Fail_When_Card_Not_Found()
        {
            _creditCardRepo.Setup(r => r.GetByCardNumberAsync("9999")).ReturnsAsync((CreditCard?)null);

            var dto = new CashierCreditCardPaymentDto { CardNumber = "9999", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.CreditCardPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("No se encontró");
        }

        [Fact]
        public async Task CreditCardPaymentAsync_Should_Fail_When_Card_Inactive()
        {
            var card = new CreditCard { Id = 1, CardNumber = "4111", CurrentDebt = 1000, Status = CreditCardStatus.Cancelled };
            _creditCardRepo.Setup(r => r.GetByCardNumberAsync("4111")).ReturnsAsync(card);

            var dto = new CashierCreditCardPaymentDto { CardNumber = "4111", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.CreditCardPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("inactiva");
        }

        [Fact]
        public async Task CreditCardPaymentAsync_Should_Fail_When_Amount_Exceeds_Debt()
        {
            var card = new CreditCard { Id = 1, CardNumber = "4111", CurrentDebt = 500, Status = CreditCardStatus.Active };
            _creditCardRepo.Setup(r => r.GetByCardNumberAsync("4111")).ReturnsAsync(card);

            var dto = new CashierCreditCardPaymentDto { CardNumber = "4111", Amount = 1000, ResponsibleUserId = "cashier1" };

            var result = await _service.CreditCardPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("supera la deuda");
        }

        [Fact]
        public async Task CreditCardPaymentAsync_Should_Fail_When_Amount_Zero()
        {
            var card = new CreditCard { Id = 1, CardNumber = "4111", CurrentDebt = 1000, Status = CreditCardStatus.Active };
            _creditCardRepo.Setup(r => r.GetByCardNumberAsync("4111")).ReturnsAsync(card);

            var dto = new CashierCreditCardPaymentDto { CardNumber = "4111", Amount = 0, ResponsibleUserId = "cashier1" };

            var result = await _service.CreditCardPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("mayor a cero");
        }

        #endregion

        #region LoanPaymentAsync

        [Fact]
        public async Task LoanPaymentAsync_Should_Return_Success_When_Valid()
        {
            var loan = new Loan { Id = 1, LoanNumber = "LN001", AmountPending = 5000, PaidInstallments = 0, Status = LoanStatus.Active, ClientId = "client1" };
            var originAccount = new SavingAccount { Id = 10, AccountNumber = "ACC001", Balance = 5000, Status = SavingAccountStatus.Active, ClientId = "client2" };
            _loanRepo.Setup(r => r.GetByLoanNumberAsync("LN001")).ReturnsAsync(loan);
            _loanRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Loan>())).ReturnsAsync(loan);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("ACC001")).ReturnsAsync(originAccount);
            _savingAccountRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<SavingAccount>())).ReturnsAsync(originAccount);
            _transactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(new Transaction { Id = 1 });

            var dto = new CashierLoanPaymentDto { LoanNumber = "LN001", Amount = 1000, OriginAccountNumber = "ACC001", ResponsibleUserId = "cashier1" };

            var result = await _service.LoanPaymentAsync(dto);

            result.Success.Should().BeTrue();
            result.OperationType.Should().Be("Pago a Préstamo");
            result.Amount.Should().Be(1000);
            result.NewBalance.Should().Be(4000);
            loan.AmountPending.Should().Be(4000);
            loan.PaidInstallments.Should().Be(1);
            originAccount.Balance.Should().Be(4000);
        }

        [Fact]
        public async Task LoanPaymentAsync_Should_Mark_Completed_When_Fully_Paid()
        {
            var loan = new Loan { Id = 1, LoanNumber = "LN002", AmountPending = 500, PaidInstallments = 11, Status = LoanStatus.Active, ClientId = "client1" };
            var originAccount = new SavingAccount { Id = 10, AccountNumber = "ACC001", Balance = 5000, Status = SavingAccountStatus.Active, ClientId = "client2" };
            _loanRepo.Setup(r => r.GetByLoanNumberAsync("LN002")).ReturnsAsync(loan);
            _loanRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Loan>())).ReturnsAsync(loan);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("ACC001")).ReturnsAsync(originAccount);
            _savingAccountRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<SavingAccount>())).ReturnsAsync(originAccount);
            _transactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(new Transaction { Id = 1 });

            var dto = new CashierLoanPaymentDto { LoanNumber = "LN002", Amount = 500, OriginAccountNumber = "ACC001", ResponsibleUserId = "cashier1" };

            var result = await _service.LoanPaymentAsync(dto);

            result.Success.Should().BeTrue();
            loan.AmountPending.Should().Be(0);
            loan.Status.Should().Be(LoanStatus.Completed);
        }

        [Fact]
        public async Task LoanPaymentAsync_Should_Fail_When_Loan_Not_Found()
        {
            _loanRepo.Setup(r => r.GetByLoanNumberAsync("NONEXIST")).ReturnsAsync((Loan?)null);

            var dto = new CashierLoanPaymentDto { LoanNumber = "NONEXIST", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.LoanPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("No se encontró");
        }

        [Fact]
        public async Task LoanPaymentAsync_Should_Fail_When_Loan_Inactive()
        {
            var loan = new Loan { Id = 1, LoanNumber = "LN003", AmountPending = 1000, Status = LoanStatus.Completed };
            _loanRepo.Setup(r => r.GetByLoanNumberAsync("LN003")).ReturnsAsync(loan);

            var dto = new CashierLoanPaymentDto { LoanNumber = "LN003", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.LoanPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("no está activo");
        }

        [Fact]
        public async Task LoanPaymentAsync_Should_Fail_When_Amount_Exceeds_Pending()
        {
            var loan = new Loan { Id = 1, LoanNumber = "LN004", AmountPending = 200, Status = LoanStatus.Active };
            _loanRepo.Setup(r => r.GetByLoanNumberAsync("LN004")).ReturnsAsync(loan);

            var dto = new CashierLoanPaymentDto { LoanNumber = "LN004", Amount = 500, ResponsibleUserId = "cashier1" };

            var result = await _service.LoanPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("supera el saldo pendiente");
        }

        [Fact]
        public async Task LoanPaymentAsync_Should_Fail_When_Amount_Zero()
        {
            var loan = new Loan { Id = 1, LoanNumber = "LN005", AmountPending = 1000, Status = LoanStatus.Active };
            _loanRepo.Setup(r => r.GetByLoanNumberAsync("LN005")).ReturnsAsync(loan);

            var dto = new CashierLoanPaymentDto { LoanNumber = "LN005", Amount = 0, ResponsibleUserId = "cashier1" };

            var result = await _service.LoanPaymentAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("mayor a cero");
        }

        #endregion

        #region TransferBetweenAccountsAsync

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Return_Success_When_Valid()
        {
            var origin = new SavingAccount { Id = 1, AccountNumber = "111", Balance = 5000, Status = SavingAccountStatus.Active };
            var destination = new SavingAccount { Id = 2, AccountNumber = "222", Balance = 1000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("111")).ReturnsAsync(origin);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("222")).ReturnsAsync(destination);
            _savingAccountRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<SavingAccount>())).ReturnsAsync(origin);
            _transactionRepo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(new Transaction { Id = 1 });

            var dto = new CashierTransferDto { OriginAccountNumber = "111", DestinationAccountNumber = "222", Amount = 2000, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeTrue();
            result.OperationType.Should().Be("Transferencia entre Cuentas");
            result.Amount.Should().Be(2000);
            result.NewBalance.Should().Be(3000);
            origin.Balance.Should().Be(3000);
            destination.Balance.Should().Be(3000);
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Fail_When_Same_Account()
        {
            var dto = new CashierTransferDto { OriginAccountNumber = "111", DestinationAccountNumber = "111", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("no pueden ser la misma");
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Fail_When_Origin_Not_Found()
        {
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("999")).ReturnsAsync((SavingAccount?)null);

            var dto = new CashierTransferDto { OriginAccountNumber = "999", DestinationAccountNumber = "222", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("cuenta de origen");
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Fail_When_Destination_Not_Found()
        {
            var origin = new SavingAccount { Id = 1, AccountNumber = "111", Balance = 5000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("111")).ReturnsAsync(origin);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("999")).ReturnsAsync((SavingAccount?)null);

            var dto = new CashierTransferDto { OriginAccountNumber = "111", DestinationAccountNumber = "999", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("cuenta de destino");
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Fail_When_Origin_Inactive()
        {
            var origin = new SavingAccount { Id = 1, AccountNumber = "111", Balance = 5000, Status = SavingAccountStatus.Cancelled };
            var dest = new SavingAccount { Id = 2, AccountNumber = "222", Balance = 1000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("111")).ReturnsAsync(origin);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("222")).ReturnsAsync(dest);

            var dto = new CashierTransferDto { OriginAccountNumber = "111", DestinationAccountNumber = "222", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("cuenta de origen está inactiva");
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Fail_When_Destination_Inactive()
        {
            var origin = new SavingAccount { Id = 1, AccountNumber = "111", Balance = 5000, Status = SavingAccountStatus.Active };
            var dest = new SavingAccount { Id = 2, AccountNumber = "222", Balance = 1000, Status = SavingAccountStatus.Cancelled };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("111")).ReturnsAsync(origin);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("222")).ReturnsAsync(dest);

            var dto = new CashierTransferDto { OriginAccountNumber = "111", DestinationAccountNumber = "222", Amount = 100, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("cuenta de destino está inactiva");
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Fail_When_Insufficient_Funds()
        {
            var origin = new SavingAccount { Id = 1, AccountNumber = "111", Balance = 100, Status = SavingAccountStatus.Active };
            var dest = new SavingAccount { Id = 2, AccountNumber = "222", Balance = 1000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("111")).ReturnsAsync(origin);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("222")).ReturnsAsync(dest);

            var dto = new CashierTransferDto { OriginAccountNumber = "111", DestinationAccountNumber = "222", Amount = 500, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Fondos insuficientes");
        }

        [Fact]
        public async Task TransferBetweenAccountsAsync_Should_Fail_When_Amount_Zero()
        {
            var origin = new SavingAccount { Id = 1, AccountNumber = "111", Balance = 5000, Status = SavingAccountStatus.Active };
            var dest = new SavingAccount { Id = 2, AccountNumber = "222", Balance = 1000, Status = SavingAccountStatus.Active };
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("111")).ReturnsAsync(origin);
            _savingAccountRepo.Setup(r => r.GetByAccountNumberAsync("222")).ReturnsAsync(dest);

            var dto = new CashierTransferDto { OriginAccountNumber = "111", DestinationAccountNumber = "222", Amount = 0, ResponsibleUserId = "cashier1" };

            var result = await _service.TransferBetweenAccountsAsync(dto);

            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("mayor a cero");
        }

        #endregion

        #region GetDailyIndicatorsAsync

        [Fact]
        public async Task GetDailyIndicatorsAsync_Should_Return_Correct_Counts()
        {
            var today = DateTime.UtcNow.Date;
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, ResponsibleUserId = "cashier1", TransactionDate = today.AddHours(9), Amount = 100, Type = TransactionType.Deposit },
                new Transaction { Id = 2, ResponsibleUserId = "cashier1", TransactionDate = today.AddHours(10), Amount = 200, Type = TransactionType.Deposit },
                new Transaction { Id = 3, ResponsibleUserId = "cashier1", TransactionDate = today.AddHours(11), Amount = 50, Type = TransactionType.Withdrawal },
                new Transaction { Id = 4, ResponsibleUserId = "cashier1", TransactionDate = today.AddHours(12), Amount = 300, Type = TransactionType.LoanPayment },
                new Transaction { Id = 5, ResponsibleUserId = "otherCashier", TransactionDate = today.AddHours(9), Amount = 999, Type = TransactionType.Deposit },
                new Transaction { Id = 6, ResponsibleUserId = "cashier1", TransactionDate = today.AddDays(-1), Amount = 500, Type = TransactionType.Deposit },
            };
            _transactionRepo.Setup(r => r.GetAllListAsync()).ReturnsAsync(transactions);

            var result = await _service.GetDailyIndicatorsAsync("cashier1");

            result.TotalDeposits.Should().Be(2);
            result.TotalWithdrawals.Should().Be(1);
            result.TotalLoanPayments.Should().Be(1);
            result.TotalTransfers.Should().Be(0);
            result.TotalAmountOperated.Should().Be(650);
        }

        [Fact]
        public async Task GetDailyIndicatorsAsync_Should_Return_Zeros_When_No_Transactions()
        {
            _transactionRepo.Setup(r => r.GetAllListAsync()).ReturnsAsync(new List<Transaction>());

            var result = await _service.GetDailyIndicatorsAsync("cashier1");

            result.TotalDeposits.Should().Be(0);
            result.TotalWithdrawals.Should().Be(0);
            result.TotalAmountOperated.Should().Be(0);
        }

        #endregion

        #region GetDailyTransactionsByCashierAsync

        [Fact]
        public async Task GetDailyTransactionsByCashierAsync_Should_Return_Filtered_Transactions()
        {
            var today = DateTime.UtcNow.Date;
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, ResponsibleUserId = "cashier1", TransactionDate = today.AddHours(10), Amount = 100, Type = TransactionType.Deposit },
                new Transaction { Id = 2, ResponsibleUserId = "cashier1", TransactionDate = today.AddHours(11), Amount = 200, Type = TransactionType.Withdrawal },
                new Transaction { Id = 3, ResponsibleUserId = "other", TransactionDate = today.AddHours(12), Amount = 999, Type = TransactionType.Deposit },
            };
            _transactionRepo.Setup(r => r.GetAllListAsync()).ReturnsAsync(transactions);
            _mapper.Setup(m => m.Map<List<TransactionDto>>(It.IsAny<List<Transaction>>()))
                .Returns((List<Transaction> src) => src.Select(t => new TransactionDto { Id = t.Id, Amount = t.Amount }).ToList());

            var result = await _service.GetDailyTransactionsByCashierAsync("cashier1");

            result.Should().HaveCount(2);
            result.Should().OnlyContain(t => t.Amount != 999);
        }

        #endregion
    }
}
