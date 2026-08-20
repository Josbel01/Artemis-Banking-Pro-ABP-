using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Helpers;
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
        private readonly IBaseAccountService _accountService;
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;
        private readonly ICreditCardService _creditCardService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public LoanController(
            ILoanService loanService, 
            IBaseAccountService accountService,
            ISavingAccountService savingAccountService,
            ITransactionService transactionService,
            ICreditCardService creditCardService,
            IEmailService emailService,
            IMapper mapper)
        {
            _loanService = loanService;
            _accountService = accountService;
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
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
                {
                    loans = loans.Where(l => l.ClientId == user.Id).ToList();
                }
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
                {
                    loans = loans.Where(l => l.Status == loanStatus).ToList();
                }
                ViewBag.Status = status;
            }
            else
            {
                if (string.IsNullOrEmpty(status))
                {
                    loans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
                }
                ViewBag.Status = status ?? "Activos";
            }

            loans = loans.OrderByDescending(l => l.Id).ToList();

            // Pagination
            int totalRecords = loans.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            var pagedLoans = loans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModels = _mapper.Map<List<LoanViewModel>>(pagedLoans);
            
            foreach (var vm in viewModels)
            {
                var client = allUsers.FirstOrDefault(u => u.Id == vm.ClientId);
                if (client != null)
                {
                    vm.ClientName = $"{client.FirstName} {client.LastName}";                }
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
            
            // Get active loans to filter out clients who already have one
            var activeLoans = await _loanService.GetAllAsync();
            var clientsWithActiveLoans = activeLoans
                .Where(l => l.Status == LoanStatus.Active)
                .Select(l => l.ClientId)
                .ToList();
            
            // Only show clients without active loans
            var availableClients = clients.Where(c => !clientsWithActiveLoans.Contains(c.Id)).ToList();
            
            // Average Debt Calculation
            var allCreditCards = await _creditCardService.GetAllAsync();
            decimal totalLoanDebt = activeLoans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.AmountPending);
            decimal totalCreditCardDebt = allCreditCards.Sum(c => c.CurrentDebt);
            decimal totalDebt = totalLoanDebt + totalCreditCardDebt;
            int activeClientsCount = clients.Count;
            ViewBag.AverageDebt = activeClientsCount > 0 ? totalDebt / activeClientsCount : 0;

            // Prepare client data with debt info
            var clientDebtInfo = availableClients.Select(c => new ClientSelectionViewModel
            {
                ClientId = c.Id,
                FullName = $"{c.FirstName} {c.LastName}",
                Email = c.Email,
                DNI = c.DNI,
                TotalDebt = CalculateClientDebt(c.Id, activeLoans, allCreditCards)
            }).ToList();

            return View(clientDebtInfo);
        }

        // STEP 1 POST: Validate selection and redirect to Step 2
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ClientSelectionInputViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedClientId))
            {
                ModelState.AddModelError("", "Debe seleccionar un cliente para continuar.");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Create));
            }

            return RedirectToAction(nameof(CreateStep2), new { clientId = model.SelectedClientId });
        }

        // ==================== STEP 2: Assignment Form ====================
        [HttpGet]
        public async Task<IActionResult> CreateStep2(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return RedirectToAction(nameof(Create));
            }

            var client = await _accountService.GetUserById(clientId);
            if (client == null || !client.IsActive)
            {
                TempData["ErrorMessage"] = "El cliente seleccionado no existe o no est\u00e1 activo.";
                return RedirectToAction(nameof(Create));
            }

            // Verify client doesn't already have an active loan
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

            // Calculate client total debt
            var allLoans = await _loanService.GetAllAsync();
            var allCards = await _creditCardService.GetAllAsync();
            ViewBag.ClientDebt = CalculateClientDebt(clientId, allLoans, allCards);

            return View(new SaveLoanViewModel { ClientId = clientId });
        }

        // STEP 2 POST: Process the assignment
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

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Verify client doesn't have an active loan
            var clientLoans = await _loanService.GetAllByClientIdAsync(vm.ClientId);
            if (clientLoans.Any(l => l.Status == LoanStatus.Active))
            {
                ModelState.AddModelError("", "Este cliente ya tiene un pr\u00e9stamo activo asignado.");
                return View(vm);
            }

            // === RISK EVALUATION ===
            var allActiveClients = await _accountService.GetAllUser(isActive: true);
            var clientUsers = allActiveClients.Where(u => u.Roles != null && u.Roles.Contains("Client")).ToList();
            var allLoans = await _loanService.GetAllAsync();
            var allCards = await _creditCardService.GetAllAsync();

            decimal totalSystemDebt = 0;
            foreach (var cu in clientUsers)
            {
                decimal loanDebt = allLoans.Where(l => l.ClientId == cu.Id && l.Status == LoanStatus.Active).Sum(l => l.AmountPending);
                decimal cardDebt = allCards.Where(c => c.ClientId == cu.Id && c.Status == CreditCardStatus.Active).Sum(c => c.CurrentDebt);
                totalSystemDebt += loanDebt + cardDebt;
            }
            decimal avgDebt = clientUsers.Count > 0 ? totalSystemDebt / clientUsers.Count : 0;

            // Current client debt
            decimal clientCurrentLoanDebt = allLoans.Where(l => l.ClientId == vm.ClientId && l.Status == LoanStatus.Active).Sum(l => l.AmountPending);
            decimal clientCurrentCardDebt = allCards.Where(c => c.ClientId == vm.ClientId && c.Status == CreditCardStatus.Active).Sum(c => c.CurrentDebt);
            decimal clientCurrentDebt = clientCurrentLoanDebt + clientCurrentCardDebt;

            // Generate amortization to calculate projected total
            var tempInstallments = LoanAmortizationCalculator.GenerateAmortizationSchedule(
                vm.PrincipalAmount, vm.InterestRate, vm.TermInMonths, DateTime.Now);
            decimal newLoanTotal = tempInstallments.Sum(i => i.InstallmentAmount);
            decimal projectedDebt = clientCurrentDebt + newLoanTotal;

            string riskMessage = null;
            if (clientCurrentDebt > avgDebt)
            {
                riskMessage = "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema.";
            }
            else if (projectedDebt > avgDebt)
            {
                riskMessage = "Asignar este pr\u00e9stamo convertir\u00e1 al cliente en un cliente de alto riesgo, ya que su deuda superar\u00e1 el umbral promedio del sistema.";
            }

            if (!string.IsNullOrEmpty(riskMessage))
            {
                // Store loan data in TempData for confirmation step
                TempData["RiskMessage"] = riskMessage;
                TempData["RiskClientId"] = vm.ClientId;
                TempData["RiskPrincipal"] = vm.PrincipalAmount.ToString();
                TempData["RiskRate"] = vm.InterestRate.ToString();
                TempData["RiskTerm"] = vm.TermInMonths.ToString();
                TempData["RiskClientName"] = $"{client.FirstName} {client.LastName}";
                TempData["RiskAvgDebt"] = avgDebt.ToString("N2");
                TempData["RiskCurrentDebt"] = clientCurrentDebt.ToString("N2");
                TempData["RiskProjectedDebt"] = projectedDebt.ToString("N2");
                return RedirectToAction(nameof(RiskConfirmation));
            }

            // No risk - proceed directly
            return await ProcessLoanCreation(vm, client, clientLoans);
        }

        [HttpGet]
        public IActionResult RiskConfirmation()
        {
            if (TempData["RiskMessage"] == null)
                return RedirectToAction(nameof(Create));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RiskConfirm(string confirm)
        {
            if (confirm != "yes")
                return RedirectToAction(nameof(Index));

            string clientId = TempData["RiskClientId"]?.ToString();
            decimal principal = decimal.Parse(TempData["RiskPrincipal"]?.ToString() ?? "0");
            decimal rate = decimal.Parse(TempData["RiskRate"]?.ToString() ?? "0");
            int term = int.Parse(TempData["RiskTerm"]?.ToString() ?? "0");

            var client = await _accountService.GetUserById(clientId);
            var clientLoans = await _loanService.GetAllByClientIdAsync(clientId);

            var vm = new SaveLoanViewModel
            {
                ClientId = clientId,
                PrincipalAmount = principal,
                InterestRate = rate,
                TermInMonths = term
            };

            return await ProcessLoanCreation(vm, client, clientLoans);
        }

        private async Task<IActionResult> ProcessLoanCreation(SaveLoanViewModel vm, UserDto client, List<LoanDto> clientLoans)
        {
            if (client == null || !client.IsActive)
            {
                TempData["ErrorMessage"] = "El cliente seleccionado no existe o no est\u00e1 activo.";
                return RedirectToAction(nameof(Create));
            }

            if (clientLoans.Any(l => l.Status == LoanStatus.Active))
            {
                TempData["ErrorMessage"] = "Este cliente ya tiene un pr\u00e9stamo activo asignado.";
                return RedirectToAction(nameof(Create));
            }

            // Generate a unique 9-digit loan number
            var rnd = new Random();
            string loanNumber = rnd.Next(100000000, 999999999).ToString();
            
            var dto = new LoanDto
            {
                Id = 0,
                ClientId = vm.ClientId,
                LoanNumber = loanNumber,
                AmountApproved = vm.PrincipalAmount,
                AmountPending = vm.PrincipalAmount,
                AnnualInterestRate = vm.InterestRate,
                TermInMonths = vm.TermInMonths,
                Status = LoanStatus.Active,
                AssignedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
            };
            
            // Generate amortization schedule
            var installments = LoanAmortizationCalculator.GenerateAmortizationSchedule(
                vm.PrincipalAmount, 
                vm.InterestRate, 
                vm.TermInMonths, 
                DateTime.Now
            );
            
            dto.AmountPending = installments.Sum(i => i.InstallmentAmount);
            dto.LoanInstallments = _mapper.Map<List<LoanInstallmentDto>>(installments);

            var createdLoan = await _loanService.AddAsync(dto);

            // Deposit to the main saving account
            var clientAccounts = await _savingAccountService.GetAllByClientIdAsync(vm.ClientId);
            var mainAccount = clientAccounts.FirstOrDefault(a => a.AccountType == SavingAccountType.Main && a.Status == SavingAccountStatus.Active);
            
            if (mainAccount != null)
            {
                mainAccount.Balance += vm.PrincipalAmount;
                await _savingAccountService.UpdateAsync(mainAccount, mainAccount.Id);

                await _transactionService.AddAsync(new TransactionDto
                {
                    SavingAccountId = mainAccount.Id,
                    Amount = vm.PrincipalAmount,
                    Type = TransactionType.Credit,
                    TransactionDate = DateTime.Now,
                    Origin = createdLoan.LoanNumber,
                    Beneficiary = mainAccount.AccountNumber,
                    Status = TransactionStatus.Approved
                });
            }
            else
            {
                TempData["WarningMessage"] = "Pr\u00e9stamo creado, pero el cliente no tiene una cuenta principal activa para el desembolso.";
            }

            // Send email to client about loan approval
            if (client != null)
            {
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
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Su pr&#233;stamo ha sido aprobado y registrado exitosamente. A continuaci&#243;n los detalles:</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">N&#250;mero de pr&#233;stamo</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">#{loanNumber}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Monto aprobado</td><td style="padding:10px 14px;font-size:18px;font-weight:700;color:#16a34a;">RD${vm.PrincipalAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Tasa de inter&#233;s anual</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{vm.InterestRate}%</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Plazo</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{vm.TermInMonths} meses</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha de aprobaci&#243;n</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{DateTime.Now:dd/MM/yyyy}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Pr&#243;xima cuota</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">RD${installments[0].InstallmentAmount:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Vencimiento pr&#243;xima cuota</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{installments[0].DueDate:dd/MM/yyyy}</td></tr>
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

                await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                {
                    To = client.Email,
                    Subject = "Pr&#233;stamo aprobado - Artemis Banking Pro",
                    HtmlBody = emailBody
                });
            }

            TempData["SuccessMessage"] = "Pr\u00e9stamo asignado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ==================== Edit Rate ====================
        [HttpGet]
        public async Task<IActionResult> EditRate(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "El préstamo indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            var client = await _accountService.GetUserById(loan.ClientId);
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "";
            ViewBag.LoanNumber = loan.LoanNumber;
            ViewBag.CurrentRate = loan.AnnualInterestRate;

            var vm = new UpdateLoanRateViewModel
            {
                Id = loan.Id,
                AnnualInterestRate = loan.AnnualInterestRate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRate(UpdateLoanRateViewModel vm)
        {
            var loan = await _loanService.GetByIdAsync(vm.Id);
            if (loan == null)
            {
                TempData["ErrorMessage"] = "El préstamo indicado no existe.";
                return RedirectToAction(nameof(Index));
            }

            var client = await _accountService.GetUserById(loan.ClientId);
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "";
            ViewBag.LoanNumber = loan.LoanNumber;
            ViewBag.CurrentRate = loan.AnnualInterestRate;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            // Recalculate only future pending installments
            ABP.Core.Application.Dtos.Loans.LoanInstallmentDto nextPendingInstallment = null;
            if (loan.LoanInstallments != null)
            {
                var pendingInstallments = loan.LoanInstallments
                    .Where(i => i.PaymentStatus == PaymentStatus.Pending)
                    .ToList();

                nextPendingInstallment = pendingInstallments.OrderBy(i => i.DueDate).FirstOrDefault();

                decimal monthlyRate = vm.AnnualInterestRate / 100m / 12m;
                foreach (var installment in pendingInstallments)
                {
                    decimal interest = installment.PendingAmount * monthlyRate;
                    installment.InterestAmount = Math.Round(interest, 2);
                    installment.CapitalAmount = Math.Round(installment.InstallmentAmount - installment.InterestAmount, 2);
                }
            }

            loan.AnnualInterestRate = vm.AnnualInterestRate;
            await _loanService.UpdateAsync(loan, loan.Id);

            // Send email to client
            if (client != null)
            {
                string nextInstallmentRow = "";
                string nextDueDateRow = "";
                if (nextPendingInstallment != null)
                {
                    nextInstallmentRow = $"<tr><td style=\"padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;\">Pr&#243;xima cuota</td><td style=\"padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;\">RD${nextPendingInstallment.InstallmentAmount:N2}</td></tr>";
                    nextDueDateRow = $"<tr><td style=\"padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;\">Vencimiento pr&#243;xima cuota</td><td style=\"padding:10px 14px;font-size:15px;color:#0b1f3a;\">{nextPendingInstallment.DueDate:dd/MM/yyyy}</td></tr>";
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

            TempData["SuccessMessage"] = "Tasa de interés actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Helper: Calculate total debt for a client
        private decimal CalculateClientDebt(string clientId, List<LoanDto> loans, List<CreditCardDto> creditCards)
        {
            decimal loanDebt = loans
                .Where(l => l.ClientId == clientId && l.Status == LoanStatus.Active)
                .Sum(l => l.AmountPending);
            
            decimal cardDebt = creditCards
                .Where(c => c.ClientId == clientId && c.Status == CreditCardStatus.Active)
                .Sum(c => c.CurrentDebt);
            
            return loanDebt + cardDebt;
        }
    }
}
