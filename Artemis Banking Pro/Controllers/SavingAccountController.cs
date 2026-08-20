using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Application.ViewModels.SavingAccounts;
using ABP.Core.Application.ViewModels.Common;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SavingAccountController : Controller
    {
        private readonly ISavingAccountService _savingAccountService;
        private readonly IBaseAccountService _accountService;
        private readonly ILoanService _loanService;
        private readonly ICreditCardService _creditCardService;
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public SavingAccountController(
            ISavingAccountService savingAccountService, 
            IBaseAccountService accountService,
            ILoanService loanService,
            ICreditCardService creditCardService,
            ITransactionService transactionService,
            IEmailService emailService,
            IMapper mapper)
        {
            _savingAccountService = savingAccountService;
            _accountService = accountService;
            _loanService = loanService;
            _creditCardService = creditCardService;
            _transactionService = transactionService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? identification, string? accountType, int page = 1)
        {
            int pageSize = 20;
            var accounts = await _savingAccountService.GetAllAsync();
            var allUsers = await _accountService.GetAllUser();

            if (!string.IsNullOrEmpty(identification))
            {
                var user = allUsers.FirstOrDefault(u => u.DNI == identification);
                if (user != null)
                {
                    accounts = accounts.Where(a => a.ClientId == user.Id).ToList();
                }
                else
                {
                    ModelState.AddModelError("", "No existe un cliente registrado con esta c\u00e9dula.");
                    accounts = new List<SavingAccountDto>();
                }
                ViewBag.Identification = identification;
            }

            if (!string.IsNullOrEmpty(accountType) && accountType != "Todas")
            {
                if (Enum.TryParse<SavingAccountType>(accountType, true, out var typeEnum))
                {
                    accounts = accounts.Where(a => a.AccountType == typeEnum).ToList();
                }
                ViewBag.AccountType = accountType;
            }
            else
            {
                ViewBag.AccountType = "Todas";
            }

            accounts = accounts.OrderByDescending(a => a.Id).ToList();

            // Pagination
            int totalRecords = accounts.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            var pagedAccounts = accounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModels = _mapper.Map<List<SavingAccountViewModel>>(pagedAccounts);
            
            foreach (var vm in viewModels)
            {
                var client = allUsers.FirstOrDefault(u => u.Id == vm.ClientId);
                if (client != null)
                {
                    vm.ClientName = $"{client.FirstName} {client.LastName}";
                }

                // Map admin name from AssignedByUserId
                var admin = allUsers.FirstOrDefault(u => u.Id == vm.AssignedByUserId);
                if (admin != null)
                {
                    vm.AdminName = $"{admin.FirstName} {admin.LastName}";
                }
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
            var activeClients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            
            // Get active loans and credit cards for debt calculation
            var activeLoans = await _loanService.GetAllAsync();
            var allCreditCards = await _creditCardService.GetAllAsync();

            // Prepare client data with debt info
            var clientDebtInfo = activeClients.Select(c => new ClientSelectionViewModel
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

            // Verify client has a main active saving account
            var clientAccounts = await _savingAccountService.GetAllByClientIdAsync(clientId);
            var hasMainAccount = clientAccounts.Any(a => a.AccountType == SavingAccountType.Main && a.Status == SavingAccountStatus.Active);
            
            if (!hasMainAccount)
            {
                TempData["ErrorMessage"] = "El cliente debe tener una cuenta de ahorro principal activa antes de asignarle una cuenta secundaria.";
                return RedirectToAction(nameof(Create));
            }

            ViewBag.ClientName = $"{client.FirstName} {client.LastName}";
            ViewBag.ClientDNI = client.DNI;
            ViewBag.ClientEmail = client.Email;
            ViewBag.ClientId = client.Id;

            // Calculate client total debt
            var activeLoans = await _loanService.GetAllAsync();
            var allCards = await _creditCardService.GetAllAsync();
            ViewBag.ClientDebt = CalculateClientDebt(clientId, activeLoans, allCards);

            return View(new SaveSavingAccountViewModel { ClientId = clientId });
        }

        // STEP 2 POST: Process the assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(SaveSavingAccountViewModel vm)
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

            // Generate a unique 9-digit account number
            var rnd = new Random();
            string accountNumber = rnd.Next(100000000, 999999999).ToString();
            
            var dto = new SavingAccountDto
            {
                Id = 0,
                ClientId = vm.ClientId,
                AccountNumber = accountNumber,
                Balance = vm.InitialBalance,
                AccountType = SavingAccountType.Secondary,
                Status = SavingAccountStatus.Active,
                AssignedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
            };
            
            await _savingAccountService.AddAsync(dto);

            // If initial balance > 0, register a CREDIT transaction
            if (vm.InitialBalance > 0)
            {
                await _transactionService.AddAsync(new TransactionDto
                {
                    SavingAccountId = dto.Id,
                    Amount = vm.InitialBalance,
                    Type = TransactionType.Credit,
                    TransactionDate = DateTime.Now,
                    Origin = accountNumber,
                    Beneficiary = accountNumber,
                    Status = TransactionStatus.Approved
                });
            }

            // Send email notification
            var emailBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#3568b2,#60a5fa);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#127974; Nueva Cuenta de Ahorro</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{client.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se le ha asignado una nueva cuenta de ahorro secundaria. A continuaci&#243;n los detalles:</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">N&#250;mero de cuenta</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;font-family:monospace;">{accountNumber}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Tipo de cuenta</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#3568b2;">Secundaria</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Balance inicial</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#16a34a;border-radius:0 6px 6px 0;">RD${vm.InitialBalance:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Estado</td><td style="padding:10px 14px;font-size:15px;color:#16a34a;font-weight:600;">Activa</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha de creaci&#243;n</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;color:#0b1f3a;border-radius:0 6px 6px 0;">{DateTime.Now:dd/MM/yyyy}</td></tr>
</table>
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
                Subject = "Nueva cuenta de ahorro asignada",
                HtmlBody = emailBody
            });

            TempData["SuccessMessage"] = "Cuenta de ahorro secundaria creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ==================== Cancel with balance transfer ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var account = await _savingAccountService.GetByIdAsync(id);
            if (account == null)
            {
                TempData["ErrorMessage"] = "La cuenta no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (account.AccountType != SavingAccountType.Secondary || account.Status != SavingAccountStatus.Active)
            {
                TempData["ErrorMessage"] = "Solo se pueden cancelar cuentas secundarias activas.";
                return RedirectToAction(nameof(Index));
            }

            // Transfer balance to main account if > 0
            if (account.Balance > 0)
            {
                var clientAccounts = await _savingAccountService.GetAllByClientIdAsync(account.ClientId);
                var mainAccount = clientAccounts.FirstOrDefault(a => a.AccountType == SavingAccountType.Main && a.Status == SavingAccountStatus.Active);

                if (mainAccount != null)
                {
                    // Debit from secondary
                    await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                    {
                        SavingAccountId = account.Id,
                        Amount = account.Balance,
                        Type = TransactionType.Debit,
                        TransactionDate = DateTime.Now,
                        Origin = account.AccountNumber,
                        Beneficiary = mainAccount.AccountNumber,
                        Status = TransactionStatus.Approved
                    });

                    // Credit to main
                    mainAccount.Balance += account.Balance;
                    await _savingAccountService.UpdateAsync(mainAccount, mainAccount.Id);

                    await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                    {
                        SavingAccountId = mainAccount.Id,
                        Amount = account.Balance,
                        Type = TransactionType.Credit,
                        TransactionDate = DateTime.Now,
                        Origin = account.AccountNumber,
                        Beneficiary = mainAccount.AccountNumber,
                        Status = TransactionStatus.Approved
                    });
                }
            }

            // Cancel the secondary account
            account.Status = SavingAccountStatus.Cancelled;
            await _savingAccountService.UpdateAsync(account, account.Id);

            TempData["SuccessMessage"] = "Cuenta secundaria cancelada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // Helper: Calculate total debt for a client
        private decimal CalculateClientDebt(string clientId, List<ABP.Core.Application.Dtos.Loans.LoanDto> loans, List<ABP.Core.Application.Dtos.CreditCards.CreditCardDto> creditCards)
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
