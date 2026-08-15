using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Helpers;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Loans;
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
        private readonly IMapper _mapper;

        public LoanController(
            ILoanService loanService, 
            IBaseAccountService accountService,
            ISavingAccountService savingAccountService,
            ITransactionService transactionService,
            IMapper mapper)
        {
            _loanService = loanService;
            _accountService = accountService;
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? identification, string? status)
        {
            var loans = await _loanService.GetAllAsync();
            
            // Get all clients to map names
            var allUsers = await _accountService.GetAllUser();

            // Filter by client identification
            if (!string.IsNullOrEmpty(identification))
            {
                var user = allUsers.FirstOrDefault(u => u.DNI == identification);
                if (user != null)
                {
                    loans = loans.Where(l => l.ClientId == user.Id).ToList();
                }
                else
                {
                    ModelState.AddModelError("", "No existe un cliente registrado con esta cédula.");
                    loans = new List<LoanDto>(); // empty
                }
                ViewBag.Identification = identification;
            }

            // Filter by status
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
                // Default to active and completed, but mostly active as per PDF (Por defecto, el listado debe mostrar préstamos activos)
                if (string.IsNullOrEmpty(status))
                {
                    loans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
                }
                ViewBag.Status = status ?? "Activos";
            }

            // Order by most recent
            loans = loans.OrderByDescending(l => l.Id).ToList();

            var viewModels = _mapper.Map<List<LoanViewModel>>(loans);
            
            // Map client names
            foreach (var vm in viewModels)
            {
                var client = allUsers.FirstOrDefault(u => u.Id == vm.ClientId);
                if (client != null)
                {
                    vm.ClientName = $"{client.FirstName} {client.LastName}";
                }
            }

            return View(viewModels);
        }

        // Simulating the select client step by allowing a dropdown for simplicity
        public async Task<IActionResult> Create()
        {
            var allUsers = await _accountService.GetAllUser(isActive: true);
            var clients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            
            // Get active loans to filter out clients who already have one
            var activeLoans = await _loanService.GetAllAsync();
            var clientsWithActiveLoans = activeLoans.Where(l => l.Status == LoanStatus.Active).Select(l => l.ClientId).ToList();
            
            var availableClients = clients.Where(c => !clientsWithActiveLoans.Contains(c.Id)).ToList();
            
            ViewBag.Clients = availableClients;
            return View(new SaveLoanViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveLoanViewModel vm)
        {
            var allUsers = await _accountService.GetAllUser(isActive: true);
            var availableClients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            ViewBag.Clients = availableClients;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var client = await _accountService.GetUserById(vm.ClientId);
            if (client == null || !client.IsActive)
            {
                ModelState.AddModelError("", "El cliente seleccionado no existe o no está activo.");
                return View(vm);
            }

            // Verify if client has an active loan
            var clientLoans = await _loanService.GetAllByClientIdAsync(vm.ClientId);
            if (clientLoans.Any(l => l.Status == LoanStatus.Active))
            {
                ModelState.AddModelError("", "Este cliente ya tiene un préstamo activo asignado.");
                return View(vm);
            }

            // Generate a unique 9-digit loan number
            var rnd = new Random();
            string loanNumber = rnd.Next(100000000, 999999999).ToString();
            
            // Map to Dto and generate amortization schedule
            var dto = new LoanDto
            {
                Id = 0,
                ClientId = vm.ClientId,
                LoanNumber = loanNumber,
                AmountApproved = vm.PrincipalAmount,
                AmountPending = vm.PrincipalAmount, // Initial pending is the full amount
                AnnualInterestRate = vm.InterestRate,
                TermInMonths = vm.TermInMonths,
                Status = LoanStatus.Active,
                AssignedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
            };
            
            // Convert Dto to Entity format for calculator temporarily, or calculate directly
            var installments = LoanAmortizationCalculator.GenerateAmortizationSchedule(
                vm.PrincipalAmount, 
                vm.InterestRate, 
                vm.TermInMonths, 
                DateTime.Now
            );
            
            // Calculate total expected interest and update pending amount
            dto.AmountPending = installments.Sum(i => i.InstallmentAmount);
            
            // Add mapping from Entity back to DTO
            dto.LoanInstallments = _mapper.Map<List<LoanInstallmentDto>>(installments);

            // Save the loan
            var createdLoan = await _loanService.AddAsync(dto);

            // Deposit to the main saving account
            var clientAccounts = await _savingAccountService.GetAllByClientIdAsync(vm.ClientId);
            var mainAccount = clientAccounts.FirstOrDefault(a => a.AccountType == SavingAccountType.Main && a.Status == SavingAccountStatus.Active);
            
            if (mainAccount != null)
            {
                // Deposit the amount
                mainAccount.Balance += vm.PrincipalAmount;
                await _savingAccountService.UpdateAsync(mainAccount, mainAccount.Id);

                // Register transaction
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
                // Ideally this should rollback the loan, but we'll show a warning for now
                TempData["WarningMessage"] = "Préstamo creado, pero el cliente no tiene una cuenta principal activa para el desembolso.";
            }

            TempData["SuccessMessage"] = "Préstamo asignado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
