using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Transactions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtemisBankingPro.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // Obtener ID del usuario actual (Temporal hasta tener Identity)
        private string GetCurrentClientId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "client-mock-123";
        }

        public async Task<IActionResult> Index(int accountId)
        {
            if (accountId == 0)
            {
                accountId = 1; // Default to 1 for UI testing since we don't have the Account Service yet
                TempData["InfoMessage"] = "Mostrando transacciones de la cuenta de prueba (ID 1).";
            }

            var transactions = await _transactionService.GetTransactionsByAccountIdAsync(accountId);
            ViewBag.AccountId = accountId;
            return View(transactions);
        }

        public IActionResult Transfer()
        {
            return View(new SaveTransferViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Transfer(SaveTransferViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var success = await _transactionService.TransferAsync(new SaveTransferDto
            {
                OriginAccountNumber = vm.OriginAccountNumber,
                DestinationAccountNumber = vm.DestinationAccountNumber,
                Amount = vm.Amount
            });

            if (!success)
            {
                ModelState.AddModelError("", "La transferencia falló. Verifique que ambas cuentas existan y que la cuenta de origen tenga fondos suficientes.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Transferencia de RD${vm.Amount} realizada exitosamente a la cuenta {vm.DestinationAccountNumber}.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult CashAdvance()
        {
            return View(new SaveCashAdvanceViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CashAdvance(SaveCashAdvanceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var success = await _transactionService.CashAdvanceAsync(new SaveCashAdvanceDto
            {
                OriginCreditCardNumber = vm.OriginCreditCardNumber,
                DestinationAccountNumber = vm.DestinationAccountNumber,
                Amount = vm.Amount
            });

            if (!success)
            {
                ModelState.AddModelError("", "El Avance de Efectivo falló. Verifique el límite de su tarjeta o la validez de la cuenta destino.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Avance de efectivo por RD${vm.Amount} aprobado y depositado en su cuenta.";
            return RedirectToAction("Index", "Home");
        }
    }
}
