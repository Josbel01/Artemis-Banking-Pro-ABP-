using ABP.Core.Application.Dtos.Cashier;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Cashier;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ArtemisBankingPro.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Cashier")]
    public class CashierController : Controller
    {
        private readonly ICashierService _cashierService;
        private readonly ISavingAccountService _savingAccountService;
        private readonly IBaseAccountService _accountService;

        public CashierController(ICashierService cashierService, ISavingAccountService savingAccountService, IBaseAccountService accountService)
        {
            _cashierService = cashierService;
            _savingAccountService = savingAccountService;
            _accountService = accountService;
        }

        private string? GetCashierUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public async Task<IActionResult> Home()
        {
            var cashierId = GetCashierUserId();
            var indicators = await _cashierService.GetDailyIndicatorsAsync(cashierId ?? string.Empty);
            var vm = CashierHomeViewModel.FromDto(indicators);
            return View(vm);
        }

        // ==================== DEPOSIT ====================

        public IActionResult Deposit() => View(new DepositViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Look up account holder name for confirmation screen
            string holderName = await GetAccountHolderName(vm.AccountNumber);

            TempData["PreConfirmData"] = JsonSerializer.Serialize(new PreConfirmOperationViewModel
            {
                OperationType = "Depósito",
                Amount = vm.Amount,
                AccountNumber = vm.AccountNumber,
                AccountHolderName = holderName
            });
            return RedirectToAction(nameof(PreConfirmOperation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeposit(string confirm)
        {
            if (confirm != "yes") return RedirectToAction(nameof(Home));

            var data = GetPreConfirmData<DepositViewModel>();
            if (data == null) return RedirectToAction(nameof(Home));

            var result = await _cashierService.DepositAsync(new CashierDepositDto
            {
                AccountNumber = data.AccountNumber,
                Amount = data.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Error al procesar el depósito.";
                return RedirectToAction(nameof(Deposit));
            }

            TempData["OperationResult"] = JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        // ==================== WITHDRAWAL ====================

        public IActionResult Withdrawal() => View(new WithdrawalViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string holderName = await GetAccountHolderName(vm.AccountNumber);

            TempData["PreConfirmData"] = JsonSerializer.Serialize(new PreConfirmOperationViewModel
            {
                OperationType = "Retiro",
                Amount = vm.Amount,
                AccountNumber = vm.AccountNumber,
                AccountHolderName = holderName
            });
            return RedirectToAction(nameof(PreConfirmOperation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmWithdrawal(string confirm)
        {
            if (confirm != "yes") return RedirectToAction(nameof(Home));

            var data = GetPreConfirmData<WithdrawalViewModel>();
            if (data == null) return RedirectToAction(nameof(Home));

            var result = await _cashierService.WithdrawalAsync(new CashierWithdrawalDto
            {
                AccountNumber = data.AccountNumber,
                Amount = data.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Error al procesar el retiro.";
                return RedirectToAction(nameof(Withdrawal));
            }

            TempData["OperationResult"] = JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        // ==================== CREDIT CARD PAYMENT ====================

        public IActionResult CreditCardPayment() => View(new CreditCardPaymentViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string holderName = await GetAccountHolderName(vm.OriginAccountNumber);

            TempData["PreConfirmData"] = JsonSerializer.Serialize(new PreConfirmOperationViewModel
            {
                OperationType = "Pago a Tarjeta de Crédito",
                Amount = vm.Amount,
                AccountNumber = vm.OriginAccountNumber,
                DestinationAccountNumber = vm.CardNumber,
                AccountHolderName = holderName
            });
            return RedirectToAction(nameof(PreConfirmOperation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCreditCardPayment(string confirm)
        {
            if (confirm != "yes") return RedirectToAction(nameof(Home));

            var data = GetPreConfirmData<CreditCardPaymentViewModel>();
            if (data == null) return RedirectToAction(nameof(Home));

            var result = await _cashierService.CreditCardPaymentAsync(new CashierCreditCardPaymentDto
            {
                CardNumber = data.CardNumber,
                Amount = data.Amount,
                OriginAccountNumber = data.OriginAccountNumber,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Error al procesar el pago a la tarjeta.";
                return RedirectToAction(nameof(CreditCardPayment));
            }

            TempData["OperationResult"] = JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        // ==================== LOAN PAYMENT ====================

        public IActionResult LoanPayment() => View(new LoanPaymentViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string holderName = await GetAccountHolderName(vm.OriginAccountNumber);

            TempData["PreConfirmData"] = JsonSerializer.Serialize(new PreConfirmOperationViewModel
            {
                OperationType = "Pago a Préstamo",
                Amount = vm.Amount,
                AccountNumber = vm.OriginAccountNumber,
                DestinationAccountNumber = vm.LoanNumber,
                AccountHolderName = holderName
            });
            return RedirectToAction(nameof(PreConfirmOperation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmLoanPayment(string confirm)
        {
            if (confirm != "yes") return RedirectToAction(nameof(Home));

            var data = GetPreConfirmData<LoanPaymentViewModel>();
            if (data == null) return RedirectToAction(nameof(Home));

            var result = await _cashierService.LoanPaymentAsync(new CashierLoanPaymentDto
            {
                LoanNumber = data.LoanNumber,
                Amount = data.Amount,
                OriginAccountNumber = data.OriginAccountNumber,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Error al procesar el pago al préstamo.";
                return RedirectToAction(nameof(LoanPayment));
            }

            TempData["OperationResult"] = JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        // ==================== TRANSFER ====================

        public IActionResult Transfer() => View(new CashierTransferViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(CashierTransferViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string originHolder = await GetAccountHolderName(vm.OriginAccountNumber);
            string destHolder = await GetAccountHolderName(vm.DestinationAccountNumber);

            TempData["PreConfirmData"] = JsonSerializer.Serialize(new PreConfirmOperationViewModel
            {
                OperationType = "Transferencia entre Cuentas",
                Amount = vm.Amount,
                AccountNumber = vm.OriginAccountNumber,
                DestinationAccountNumber = vm.DestinationAccountNumber,
                AccountHolderName = originHolder,
                DestinationHolderName = destHolder
            });
            return RedirectToAction(nameof(PreConfirmOperation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmTransfer(string confirm)
        {
            if (confirm != "yes") return RedirectToAction(nameof(Home));

            var data = GetPreConfirmData<CashierTransferViewModel>();
            if (data == null) return RedirectToAction(nameof(Home));

            var result = await _cashierService.TransferBetweenAccountsAsync(new CashierTransferDto
            {
                OriginAccountNumber = data.OriginAccountNumber,
                DestinationAccountNumber = data.DestinationAccountNumber,
                Amount = data.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Error al procesar la transferencia.";
                return RedirectToAction(nameof(Transfer));
            }

            TempData["OperationResult"] = JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        // ==================== PRE-CONFIRMATION ====================

        public IActionResult PreConfirmOperation()
        {
            var json = TempData.Peek("PreConfirmData")?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction(nameof(Home));

            var vm = JsonSerializer.Deserialize<PreConfirmOperationViewModel>(json);
            if (vm == null) return RedirectToAction(nameof(Home));

            // Keep so ConfirmXxx POST can read it
            TempData.Keep();
            return View(vm);
        }

        // ==================== RESULT CONFIRMATION ====================

        public IActionResult Confirmation()
        {
            var json = TempData["OperationResult"]?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction(nameof(Home));

            var result = JsonSerializer.Deserialize<OperationResultDto>(json);
            if (result == null) return RedirectToAction(nameof(Home));

            var vm = new ConfirmationViewModel
            {
                OperationType = result.OperationType,
                Amount = result.Amount,
                AccountNumber = result.AccountNumber,
                DestinationAccountNumber = result.DestinationAccountNumber,
                AccountHolderName = result.AccountHolderName,
                DestinationHolderName = result.DestinationHolderName,
                NewBalance = result.NewBalance,
                OperationDate = result.OperationDate,
                TransactionId = result.TransactionId
            };
            return View(vm);
        }

        // ==================== HISTORY ====================

        public async Task<IActionResult> History()
        {
            var cashierId = GetCashierUserId();
            var transactions = await _cashierService.GetDailyTransactionsByCashierAsync(cashierId ?? string.Empty);
            var vm = CashierHistoryViewModel.FromDtoList(transactions);
            return View(vm);
        }

        // ==================== HELPERS ====================

        private async Task<string> GetAccountHolderName(string accountNumber)
        {
            try
            {
                var account = await _savingAccountService.GetByAccountNumberAsync(accountNumber);
                if (account == null) return "";
                var user = await _accountService.GetUserById(account.ClientId);
                return user != null ? $"{user.FirstName} {user.LastName}" : "";
            }
            catch
            {
                return "";
            }
        }

        private T? GetPreConfirmData<T>() where T : class
        {
            var json = TempData["PreConfirmData"]?.ToString();
            if (string.IsNullOrEmpty(json)) return null;
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
