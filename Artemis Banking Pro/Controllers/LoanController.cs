using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Loans;
using ABP.Core.Application.ViewModels.Common;
using ABP.Core.Domain.Common.Enums;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly IBaseAccountService _accountService;
        private readonly ICreditCardService _creditCardService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public LoanController(
            ILoanService loanService,
            ILoanInstallmentService loanInstallmentService,
            IBaseAccountService accountService,
            ICreditCardService creditCardService,
            IEmailService emailService,
            IMapper mapper)
        {
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
            _accountService = accountService;
            _creditCardService = creditCardService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? identification, string? status, int page = 1)
        {
            int pageSize = 20;
            var loans = await _loanService.GetAllAsync();
            var allUsers = await _accountService.GetAllUser();

            if (!string.IsNullOrEmpty(identification))
            {
                var user = allUsers.FirstOrDefault(u => u.DNI == identification);
                if (user != null)
                    loans = loans.Where(l => l.ClientId == user.Id).ToList();
                else
                {
                    ModelState.AddModelError("", "No existe un cliente registrado con esta c\u00e9dula.");
                    loans = new List<LoanDto>();
                }
                ViewBag.Identification = identification;
            }

            if (!string.IsNullOrEmpty(status) && status != "Todos")
            {
                if (Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
                    loans = loans.Where(l => l.Status == loanStatus).ToList();
                ViewBag.Status = status;
            }
            else
            {
                if (string.IsNullOrEmpty(status))
                    loans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
                ViewBag.Status = status ?? "Activos";
            }

            loans = loans.OrderByDescending(l => l.Id).ToList();

            int totalRecords = loans.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            var pagedLoans = loans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModels = _mapper.Map<List<LoanViewModel>>(pagedLoans);
            foreach (var vm in viewModels)
            {
                var client = allUsers.FirstOrDefault(u => u.Id == vm.ClientId);
                if (client != null) vm.ClientName = $"{client.FirstName} {client.LastName}";

                // Fix old loans that have TotalInstallments = 0
                if (vm.TotalInstallments == 0)
                    vm.TotalInstallments = vm.TermInMonths;
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            return View(viewModels);
        }

        // ==================== STEP 1: Client Selection ====================
        public async Task<IActionResult> Create()
        {
            var allUsers = await _accountService.GetAllUser(isActive: true);
            var clients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            var activeLoans = await _loanService.GetAllAsync();
            var clientsWithActiveLoans = activeLoans.Where(l => l.Status == LoanStatus.Active).Select(l => l.ClientId).ToList();
            var availableClients = clients.Where(c => !clientsWithActiveLoans.Contains(c.Id)).ToList();

            var allCreditCards = await _creditCardService.GetAllAsync();
            decimal totalLoanDebt = activeLoans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.AmountPending);
            decimal totalCreditCardDebt = allCreditCards.Sum(c => c.CurrentDebt);
            ViewBag.AverageDebt = clients.Count > 0 ? (totalLoanDebt + totalCreditCardDebt) / clients.Count : 0;

            var clientDebtInfo = availableClients.Select(c => new ClientSelectionViewModel
            {
                ClientId = c.Id,
                FullName = $"{c.FirstName} {c.LastName}",
                Email = c.Email,
                DNI = c.DNI,
                TotalDebt = _loanService.CalculateClientDebt(c.Id, activeLoans, allCreditCards)
            }).ToList();

            return View(clientDebtInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClientSelectionInputViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedClientId))
                ModelState.AddModelError("", "Debe seleccionar un cliente para continuar.");
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Create));
            return RedirectToAction(nameof(CreateStep2), new { clientId = model.SelectedClientId });
        }

        // ==================== STEP 2: Assignment Form ====================
        [HttpGet]
        public async Task<IActionResult> CreateStep2(string clientId)
        {
            if (string.IsNullOrEmpty(clientId)) return RedirectToAction(nameof(Create));

            var client = await _accountService.GetUserById(clientId);
            if (client == null || !client.IsActive)
            {
                TempData["ErrorMessage"] = "El cliente seleccionado no existe o no est\u00e1 activo.";
                return RedirectToAction(nameof(Create));
            }

            var clientLoans = await _loanService.GetAllByClientIdAsync(clientId);
            if (clientLoans.Any(l => l.Status == LoanStatus.Active))
            {
                TempData["ErrorMessage"] = "Este cliente ya tiene un pr\u00e9stamo activo asignado.";
                return RedirectToAction(nameof(Create));
            }

            ViewBag.ClientName = $"{client.FirstName} {client.LastName}";
            ViewBag.ClientDNI = client.DNI;
            ViewBag.ClientEmail = client.Email;
            ViewBag.ClientId = client.Id;

            var allLoans = await _loanService.GetAllAsync();
            var allCards = await _creditCardService.GetAllAsync();
            ViewBag.ClientDebt = _loanService.CalculateClientDebt(clientId, allLoans, allCards);

            return View(new SaveLoanViewModel { ClientId = clientId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(SaveLoanViewModel vm)
        {
            var client = await _accountService.GetUserById(vm.ClientId);
            if (client == null || !client.IsActive)
            {
                TempData["ErrorMessage"] = "El cliente seleccionado no existe o no est\u00e1 activo.";
                return RedirectToAction(nameof(Create));
            }

            ViewBag.ClientName = $"{client.FirstName} {client.LastName}";
            ViewBag.ClientDNI = client.DNI;
            ViewBag.ClientEmail = client.Email;
            ViewBag.ClientId = client.Id;

            if (!ModelState.IsValid) return View(vm);

            var clientLoans = await _loanService.GetAllByClientIdAsync(vm.ClientId);
            if (clientLoans.Any(l => l.Status == LoanStatus.Active))
            {
                ModelState.AddModelError("", "Este cliente ya tiene un pr\u00e9stamo activo asignado.");
                return View(vm);
            }

            // Risk evaluation via service
            var risk = await _loanService.EvaluateRiskAsync(vm.ClientId, vm.PrincipalAmount, vm.InterestRate, vm.TermInMonths);

            if (risk.hasRisk)
            {
                TempData["RiskMessage"] = risk.message;
                TempData["RiskClientId"] = vm.ClientId;
                TempData["RiskPrincipal"] = vm.PrincipalAmount.ToString();
                TempData["RiskRate"] = vm.InterestRate.ToString();
                TempData["RiskTerm"] = vm.TermInMonths.ToString();
                TempData["RiskClientName"] = $"{client.FirstName} {client.LastName}";
                TempData["RiskAvgDebt"] = risk.avgDebt.ToString("N2");
                TempData["RiskCurrentDebt"] = risk.currentDebt.ToString("N2");
                TempData["RiskProjectedDebt"] = risk.projectedDebt.ToString("N2");
                return RedirectToAction(nameof(RiskConfirmation));
            }

            return await ProcessLoanCreation(vm, client);
        }

        [HttpGet]
        public IActionResult RiskConfirmation()
        {
            if (TempData.Peek("RiskMessage") == null) return RedirectToAction(nameof(Create));
            TempData.Keep();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RiskConfirm(string confirm)
        {
            if (confirm != "yes") return RedirectToAction(nameof(Index));

            try
            {
                string clientId = TempData["RiskClientId"]?.ToString();
                decimal principal = decimal.Parse(TempData["RiskPrincipal"]?.ToString() ?? "0");
                decimal rate = decimal.Parse(TempData["RiskRate"]?.ToString() ?? "0");
                int term = int.Parse(TempData["RiskTerm"]?.ToString() ?? "0");

                var client = await _accountService.GetUserById(clientId);
                var vm = new SaveLoanViewModel { ClientId = clientId, PrincipalAmount = principal, InterestRate = rate, TermInMonths = term };
                return await ProcessLoanCreation(vm, client);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al confirmar el pr\u00e9stamo: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        private async Task<IActionResult> ProcessLoanCreation(SaveLoanViewModel vm, UserDto client)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
                var createdLoan = await _loanService.ProcessLoanCreationAsync(vm.ClientId, vm.PrincipalAmount, vm.InterestRate, vm.TermInMonths, userId);

                if (createdLoan == null)
                {
                    TempData["ErrorMessage"] = "Error al crear el pr\u00e9stamo. Verifique los datos e intente nuevamente.";
                    return RedirectToAction(nameof(Create));
                }

                // Send email (presentation logic stays in controller)
                if (client != null)
                {
                    var installments = await _loanService.GetAllByClientIdAsync(vm.ClientId);
                    var loan = installments.FirstOrDefault(l => l.Id == createdLoan.Id);

                    var emailBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#16a34a,#22c55e);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#9989; Pr&#233;stamo Aprobado</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{client.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Su pr&#233;stamo ha sido aprobado y registrado exitosamente.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">N&#250;mero de pr&#233;stamo</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">#{createdLoan.LoanNumber}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto aprobado</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#16a34a;">RD${vm.PrincipalAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Tasa de inter&#233;s anual</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{vm.InterestRate}%</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Plazo</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{vm.TermInMonths} meses</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha de aprobaci&#243;n</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{DateTime.Now:dd/MM/yyyy}</td></tr>
</table>
<div style="background:#dcfce7;border-left:4px solid #16a34a;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#166534;font-size:13px;margin:0;">&#128176; El monto ha sido depositado en su cuenta de ahorro principal.</p>
</div>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
""";

                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = client.Email,
                        Subject = "Pr&#233;stamo aprobado - Artemis Banking Pro",
                        HtmlBody = emailBody
                    });
                }

                TempData["SuccessMessage"] = "Pr\u00e9stamo asignado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear el pr\u00e9stamo: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        // ==================== Edit Rate ====================
        [HttpGet]
        public async Task<IActionResult> EditRate(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "El pr\u00e9stamo indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            var client = await _accountService.GetUserById(loan.ClientId);
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "";
            ViewBag.LoanNumber = loan.LoanNumber;
            ViewBag.CurrentRate = loan.AnnualInterestRate;

            var vm = new UpdateLoanRateViewModel { Id = loan.Id, AnnualInterestRate = loan.AnnualInterestRate };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRate(UpdateLoanRateViewModel vm)
        {
            var loan = await _loanService.GetByIdAsync(vm.Id);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "El pr\u00e9stamo indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            var client = await _accountService.GetUserById(loan.ClientId);
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "";
            ViewBag.LoanNumber = loan.LoanNumber;
            ViewBag.CurrentRate = loan.AnnualInterestRate;

            if (!ModelState.IsValid) return View(vm);

            // Recalculate future installments with new rate
            // Get all installments for this loan
            var allInstallments = await _loanInstallmentService.GetAllByLoanIdAsync(loan.Id);
            ABP.Core.Application.Dtos.Loans.LoanInstallmentDto nextPendingInstallment = null;

            if (allInstallments != null && allInstallments.Count > 0)
            {
                decimal monthlyRate = vm.AnnualInterestRate / 100m / 12m;

                // Calculate remaining principal: sum of unpaid capital amounts
                decimal remainingPrincipal = allInstallments
                    .Where(i => i.PaymentStatus != PaymentStatus.Paid)
                    .Sum(i => i.CapitalAmount);

                // Recalculate all unpaid installments with new rate (French amortization)
                var unpaidInstallments = allInstallments
                    .Where(i => i.PaymentStatus != PaymentStatus.Paid)
                    .OrderBy(i => i.InstallmentNumber)
                    .ToList();

                nextPendingInstallment = unpaidInstallments.FirstOrDefault();

                foreach (var installment in unpaidInstallments)
                {
                    // Interest = remaining principal * monthly rate
                    decimal interest = remainingPrincipal * monthlyRate;
                    installment.InterestAmount = Math.Round(interest, 2);

                    // For the last installment, capital = remaining principal to close at 0
                    if (installment == unpaidInstallments.Last())
                    {
                        installment.CapitalAmount = remainingPrincipal;
                    }
                    else
                    {
                        // Capital portion (distribute evenly or proportional)
                        decimal capitalPerInstallment = remainingPrincipal / unpaidInstallments.Count;
                        installment.CapitalAmount = Math.Round(capitalPerInstallment, 2);
                    }

                    installment.InstallmentAmount = Math.Round(installment.InterestAmount + installment.CapitalAmount, 2);
                    installment.PendingAmount = installment.InstallmentAmount; // Reset pending to full amount

                    await _loanInstallmentService.UpdateAsync(installment, installment.Id);

                    // Reduce remaining principal by this installment's capital
                    remainingPrincipal -= installment.CapitalAmount;
                }

                // Update loan total pending amount
                loan.AmountPending = allInstallments.Where(i => i.PaymentStatus != PaymentStatus.Paid).Sum(i => i.PendingAmount);
            }

            loan.AnnualInterestRate = vm.AnnualInterestRate;
            await _loanService.UpdateAsync(loan, loan.Id);

            // Send email
            if (client != null)
            {
                // Re-fetch the recalculated installment to get updated values
                var freshInstallments = await _loanInstallmentService.GetAllByLoanIdAsync(loan.Id);
                var updatedNextPending = freshInstallments?
                    .Where(i => i.PaymentStatus != PaymentStatus.Paid)
                    .OrderBy(i => i.InstallmentNumber)
                    .FirstOrDefault();

                string nextInstallmentRow = "";
                string nextDueDateRow = "";
                if (updatedNextPending != null)
                {
                    nextInstallmentRow = $"<tr><td style=\"padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;\">Nuevo valor de la pr&#243;xima cuota</td><td style=\"padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;\">RD${updatedNextPending.InstallmentAmount:N2}</td></tr>";
                    nextDueDateRow = $"<tr><td style=\"padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;\">Fecha de vencimiento pr&#243;xima cuota</td><td style=\"padding:10px 14px;font-size:15px;color:#0b1f3a;\">{updatedNextPending.DueDate:dd/MM/yyyy}</td></tr>";
                }

                var emailBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#7c3aed,#a78bfa);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128200; Modificaci&#243;n de Tasa</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{client.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">La tasa de inter&#233;s de su pr&#233;stamo <strong>#{loan.LoanNumber}</strong> ha sido modificada.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Nueva tasa anual</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#7c3aed;border-radius:0 6px 6px 0;">{vm.AnnualInterestRate}%</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha de cambio</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{DateTime.Now:dd/MM/yyyy}</td></tr>
{nextInstallmentRow}
{nextDueDateRow}
</table>
<div style="background:#fee2e2;border-left:4px solid #dc2626;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#991b1b;font-size:13px;margin:0;">&#128683; Si usted no reconoce esta modificaci&#243;n, comun&#237;quese con la entidad bancaria.</p>
</div>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
""";

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = client.Email,
                    Subject = "Modificaci&#243;n de tasa de inter&#233;s - Pr&#233;stamo",
                    HtmlBody = emailBody
                });
            }

            TempData["SuccessMessage"] = "Tasa de inter\u00e9s actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
