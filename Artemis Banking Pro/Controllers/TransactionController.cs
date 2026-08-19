using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Transactions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;

        public TransactionController(ITransactionService transactionService, IMapper mapper)
        {
            _transactionService = transactionService;
            _mapper = mapper;
        }

        private string? GetCurrentClientId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IActionResult> Index(int? accountId)
        {
            // Admin can view all transactions without filtering by account
            if (User.IsInRole("Admin") && (accountId == null || accountId == 0))
            {
                var allDtos = await _transactionService.GetAllAsync();
                var allViewModels = _mapper.Map<IEnumerable<TransactionViewModel>>(allDtos);
                ViewBag.AccountId = 0;
                ViewBag.IsAdminView = true;
                return View(allViewModels);
            }

            if (accountId == null || accountId == 0)
            {
                return RedirectToAction("Index", "Client");
            }

            var dtos = await _transactionService.GetTransactionsByAccountIdAsync(accountId.Value);
            var viewModels = _mapper.Map<IEnumerable<TransactionViewModel>>(dtos);
            ViewBag.AccountId = accountId;
            ViewBag.IsAdminView = false;
            return View(viewModels);
        }

        public IActionResult Transfer()
        {
            return View(new SaveTransferViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(SaveTransferViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = _mapper.Map<SaveTransferDto>(vm);
            var success = await _transactionService.TransferAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", "La transferencia fallÃ³. Verifique que ambas cuentas existan y que la cuenta de origen tenga fondos suficientes.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Transferencia de RD${vm.Amount} realizada exitosamente a la cuenta {vm.DestinationAccountNumber}.";
            return RedirectToAction("Index", "Client");
        }

        public IActionResult CashAdvance()
        {
            return View(new SaveCashAdvanceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CashAdvance(SaveCashAdvanceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = _mapper.Map<SaveCashAdvanceDto>(vm);
            var success = await _transactionService.CashAdvanceAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", "El Avance de Efectivo fallÃ³. Verifique el lÃ­mite de su tarjeta o la validez de la cuenta destino.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Avance de efectivo por RD${vm.Amount} aprobado y depositado en su cuenta.";
            return RedirectToAction("Index", "Client");
        }
    }
}

