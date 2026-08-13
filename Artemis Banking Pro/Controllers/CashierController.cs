using ABP.Core.Application.Dtos.Cashier;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Cashier;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.Controllers
{
    public class CashierController : Controller
    {
        private readonly ICashierService _cashierService;

        public CashierController(ICashierService cashierService)
        {
            _cashierService = cashierService;
        }

        private string? GetCashierUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public async Task<IActionResult> Home()
        {
            var cashierId = GetCashierUserId();
            var indicators = await _cashierService.GetDailyIndicatorsAsync(cashierId ?? string.Empty);
            var vm = CashierHomeViewModel.FromDto(indicators);
            return View(vm);
        }

        public IActionResult Deposit() => View(new DepositViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _cashierService.DepositAsync(new CashierDepositDto
            {
                AccountNumber = vm.AccountNumber,
                Amount = vm.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Error al procesar el depósito.");
                return View(vm);
            }

            TempData["OperationResult"] = System.Text.Json.JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        public IActionResult Withdrawal() => View(new WithdrawalViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _cashierService.WithdrawalAsync(new CashierWithdrawalDto
            {
                AccountNumber = vm.AccountNumber,
                Amount = vm.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Error al procesar el retiro.");
                return View(vm);
            }

            TempData["OperationResult"] = System.Text.Json.JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        public IActionResult CreditCardPayment() => View(new CreditCardPaymentViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _cashierService.CreditCardPaymentAsync(new CashierCreditCardPaymentDto
            {
                CardNumber = vm.CardNumber,
                Amount = vm.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Error al procesar el pago a la tarjeta.");
                return View(vm);
            }

            TempData["OperationResult"] = System.Text.Json.JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        public IActionResult LoanPayment() => View(new LoanPaymentViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _cashierService.LoanPaymentAsync(new CashierLoanPaymentDto
            {
                LoanNumber = vm.LoanNumber,
                Amount = vm.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Error al procesar el pago al préstamo.");
                return View(vm);
            }

            TempData["OperationResult"] = System.Text.Json.JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        public IActionResult Transfer() => View(new CashierTransferViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(CashierTransferViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _cashierService.TransferBetweenAccountsAsync(new CashierTransferDto
            {
                OriginAccountNumber = vm.OriginAccountNumber,
                DestinationAccountNumber = vm.DestinationAccountNumber,
                Amount = vm.Amount,
                ResponsibleUserId = GetCashierUserId()
            });

            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Error al procesar la transferencia.");
                return View(vm);
            }

            TempData["OperationResult"] = System.Text.Json.JsonSerializer.Serialize(result);
            return RedirectToAction(nameof(Confirmation));
        }

        public IActionResult Confirmation()
        {
            var json = TempData["OperationResult"]?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction(nameof(Home));

            var result = System.Text.Json.JsonSerializer.Deserialize<OperationResultDto>(json);
            if (result == null) return RedirectToAction(nameof(Home));

            var vm = new ConfirmationViewModel
            {
                OperationType = result.OperationType,
                Amount = result.Amount,
                AccountNumber = result.AccountNumber,
                DestinationAccountNumber = result.DestinationAccountNumber,
                NewBalance = result.NewBalance,
                OperationDate = result.OperationDate,
                TransactionId = result.TransactionId
            };
            return View(vm);
        }

        public async Task<IActionResult> History()
        {
            var cashierId = GetCashierUserId();
            var transactions = await _cashierService.GetDailyTransactionsByCashierAsync(cashierId ?? string.Empty);
            var vm = CashierHistoryViewModel.FromDtoList(transactions);
            return View(vm);
        }
    }
}
