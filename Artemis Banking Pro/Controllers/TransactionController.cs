using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Transactions;
using ABP.Core.Domain.Common.Enums;
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
        private readonly ISavingAccountService _savingAccountService;
        private readonly ICreditCardService _creditCardService;
        private readonly ILoanService _loanService;
        private readonly IBaseAccountService _accountService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public TransactionController(
            ITransactionService transactionService,
            ISavingAccountService savingAccountService,
            ICreditCardService creditCardService,
            ILoanService loanService,
            IBaseAccountService accountService,
            IEmailService emailService,
            IMapper mapper)
        {
            _transactionService = transactionService;
            _savingAccountService = savingAccountService;
            _creditCardService = creditCardService;
            _loanService = loanService;
            _accountService = accountService;
            _emailService = emailService;
            _mapper = mapper;
        }

        private string? GetCurrentClientId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IActionResult> Index(int? accountId)
        {
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

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> MyHistory()
        {
            var clientId = GetCurrentClientId();
            if (string.IsNullOrEmpty(clientId)) return RedirectToAction("Index", "Account");

            var clientAccounts = await _savingAccountService.GetAllByClientIdAsync(clientId);
            var accountIds = clientAccounts.Select(a => a.Id).ToHashSet();

            var allDtos = await _transactionService.GetAllAsync();
            var filteredDtos = allDtos.Where(t => accountIds.Contains(t.SavingAccountId));
            var viewModels = _mapper.Map<IEnumerable<TransactionViewModel>>(filteredDtos);

            ViewBag.AccountId = 0;
            ViewBag.IsAdminView = false;
            ViewBag.IsClientHistory = true;
            return View("Index", viewModels);
        }

        // ─── TRANSFER ───────────────────────────────────────────

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Transfer(string? dest)
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();

            var vm = new SaveTransferViewModel();
            if (!string.IsNullOrEmpty(dest))
            {
                vm.DestinationAccountNumber = dest;
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Transfer(SaveTransferViewModel vm)
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Validate destination account exists and is active
            var destAccount = await _savingAccountService.GetByAccountNumberAsync(vm.DestinationAccountNumber);
            if (destAccount == null)
            {
                ModelState.AddModelError("DestinationAccountNumber", "La cuenta de destino no existe.");
                return View(vm);
            }
            if (destAccount.Status != SavingAccountStatus.Active)
            {
                ModelState.AddModelError("DestinationAccountNumber", "La cuenta de destino no está activa.");
                return View(vm);
            }

            // Validate not self-transfer
            var originAccount = accounts.FirstOrDefault(a => a.AccountNumber == vm.OriginAccountNumber);
            if (originAccount != null && originAccount.Id == destAccount.Id)
            {
                ModelState.AddModelError("DestinationAccountNumber", "No puede transferir a su propia cuenta.");
                return View(vm);
            }

            // Store confirmation data and redirect to confirmation page
            TempData["ConfirmOriginAccount"] = vm.OriginAccountNumber;
            TempData["ConfirmDestAccount"] = vm.DestinationAccountNumber;
            TempData["ConfirmAmount"] = vm.Amount;
            TempData["ConfirmDestName"] = $"{destAccount.AccountNumber}";
            return RedirectToAction(nameof(TransferConfirm));
        }

        [Authorize(Roles = "Client")]
        public IActionResult TransferConfirm()
        {
            if (TempData["ConfirmDestAccount"] == null)
                return RedirectToAction(nameof(Transfer));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> TransferConfirm(string confirm)
        {
            if (confirm != "yes")
                return RedirectToAction(nameof(Transfer));

            var vm = new SaveTransferViewModel
            {
                OriginAccountNumber = TempData["ConfirmOriginAccount"]?.ToString() ?? "",
                DestinationAccountNumber = TempData["ConfirmDestAccount"]?.ToString() ?? "",
                Amount = decimal.Parse(TempData["ConfirmAmount"]?.ToString() ?? "0")
            };

            var dto = _mapper.Map<SaveTransferDto>(vm);
            var success = await _transactionService.TransferAsync(dto);

            if (!success)
            {
                TempData["ErrorMessage"] = "La transferencia falló. Verifique los datos e intente de nuevo.";
                return RedirectToAction(nameof(Transfer));
            }

            // Send email to beneficiary
            try
            {
                var destAccount = await _savingAccountService.GetByAccountNumberAsync(vm.DestinationAccountNumber);
                if (destAccount != null)
                {
                    var destUser = await _accountService.GetUserById(destAccount.ClientId);
                    if (destUser != null)
                    {
                        var originAccount = await _savingAccountService.GetByAccountNumberAsync(vm.OriginAccountNumber);
                        var lastFourOrigin = originAccount != null ? vm.OriginAccountNumber.Substring(Math.Max(0, vm.OriginAccountNumber.Length - 4)) : "****";
                        await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                        {
                            To = destUser.Email,
                            Subject = $"Transferencia recibida de RD${vm.Amount:N2}",
                            HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#0891b2,#22d3ee);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128176; Transferencia Recibida</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{destUser.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Ha recibido una transferencia en su cuenta.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Monto recibido</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#16a34a;border-radius:0 6px 6px 0;">RD${vm.Amount:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Cuenta destino</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">&#9679;&#9679;&#9679;&#9679; {vm.DestinationAccountNumber.Substring(Math.Max(0, vm.DestinationAccountNumber.Length - 4))}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Cuenta origen</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">&#9679;&#9679;&#9679;&#9679; {lastFourOrigin}</td></tr>
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
            }
            catch (Exception ex)
            {
                // Don't fail the transfer if email fails
            }

            TempData["SuccessMessage"] = $"Transferencia de RD${vm.Amount:N2} realizada exitosamente a la cuenta {vm.DestinationAccountNumber}.";
            return RedirectToAction("Index", "Client");
        }

        // ─── CASH ADVANCE ──────────────────────────────────────

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CashAdvance()
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            var cards = await _creditCardService.GetAllByClientIdAsync(clientId ?? "");

            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            ViewBag.ClientCards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();

            return View(new SaveCashAdvanceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CashAdvance(SaveCashAdvanceViewModel vm)
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            var cards = await _creditCardService.GetAllByClientIdAsync(clientId ?? "");

            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            ViewBag.ClientCards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = _mapper.Map<SaveCashAdvanceDto>(vm);
            var success = await _transactionService.CashAdvanceAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", "El Avance de Efectivo falló. Verifique el límite de su tarjeta o la validez de la cuenta destino.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Avance de efectivo por RD${vm.Amount} aprobado y depositado en su cuenta.";
            return RedirectToAction("Index", "Client");
        }

        // ─── CREDIT CARD PAYMENT ───────────────────────────────

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CreditCardPayment()
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            var cards = await _creditCardService.GetAllByClientIdAsync(clientId ?? "");

            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            ViewBag.ClientCards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();

            return View(new SaveCreditCardPaymentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> CreditCardPayment(SaveCreditCardPaymentViewModel vm)
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            var cards = await _creditCardService.GetAllByClientIdAsync(clientId ?? "");

            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            ViewBag.ClientCards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new SaveCreditCardPaymentDto
            {
                CreditCardNumber = vm.CreditCardNumber,
                OriginAccountNumber = vm.OriginAccountNumber,
                Amount = vm.Amount
            };
            var success = await _transactionService.CreditCardPaymentAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", "El pago a tarjeta de crédito falló. Verifique que la tarjeta tenga deuda pendiente y que la cuenta tenga fondos suficientes.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Pago de RD${vm.Amount} a tarjeta realizado exitosamente.";
            return RedirectToAction("Index", "Client");
        }

        // ─── LOAN PAYMENT ──────────────────────────────────────

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> LoanPayment()
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            var loans = await _loanService.GetAllByClientIdAsync(clientId ?? "");

            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            ViewBag.ClientLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();

            return View(new SaveLoanPaymentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> LoanPayment(SaveLoanPaymentViewModel vm)
        {
            var clientId = GetCurrentClientId();
            var accounts = await _savingAccountService.GetAllByClientIdAsync(clientId ?? "");
            var loans = await _loanService.GetAllByClientIdAsync(clientId ?? "");

            ViewBag.ClientAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            ViewBag.ClientLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var dto = new SaveLoanPaymentDto
            {
                LoanNumber = vm.LoanNumber,
                OriginAccountNumber = vm.OriginAccountNumber,
                Amount = vm.Amount
            };
            var success = await _transactionService.LoanPaymentAsync(dto);

            if (!success)
            {
                ModelState.AddModelError("", "El pago al préstamo falló. Verifique que el préstamo tenga cuotas pendientes y que la cuenta tenga fondos suficientes.");
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Pago de RD${vm.Amount} al préstamo realizado exitosamente.";
            return RedirectToAction("Index", "Client");
        }
    }
}
