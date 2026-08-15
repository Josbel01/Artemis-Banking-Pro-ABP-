using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.SavingAccounts;
using ABP.Core.Domain.Common.Enums;
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
        private readonly IMapper _mapper;

        public SavingAccountController(
            ISavingAccountService savingAccountService, 
            IBaseAccountService accountService,
            IMapper mapper)
        {
            _savingAccountService = savingAccountService;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? identification, string? accountType)
        {
            var accounts = await _savingAccountService.GetAllAsync();
            
            // Get all clients to map names
            var allUsers = await _accountService.GetAllUser();

            // Filter by client identification
            if (!string.IsNullOrEmpty(identification))
            {
                var user = allUsers.FirstOrDefault(u => u.DNI == identification);
                if (user != null)
                {
                    accounts = accounts.Where(a => a.ClientId == user.Id).ToList();
                }
                else
                {
                    ModelState.AddModelError("", "No existe un cliente registrado con esta cédula.");
                    accounts = new List<SavingAccountDto>(); // empty
                }
                ViewBag.Identification = identification;
            }

            // Filter by type
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

            // Order by most recent
            accounts = accounts.OrderByDescending(a => a.Id).ToList();

            var viewModels = _mapper.Map<List<SavingAccountViewModel>>(accounts);
            
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

        // Assign a secondary saving account to a client
        public async Task<IActionResult> Create()
        {
            var allUsers = await _accountService.GetAllUser(isActive: true);
            var activeClients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            
            ViewBag.Clients = activeClients;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string clientId)
        {
            var allUsers = await _accountService.GetAllUser(isActive: true);
            var activeClients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            ViewBag.Clients = activeClients;

            if (string.IsNullOrEmpty(clientId))
            {
                ModelState.AddModelError("", "Debe seleccionar un cliente.");
                return View();
            }

            var client = await _accountService.GetUserById(clientId);
            if (client == null || !client.IsActive)
            {
                ModelState.AddModelError("", "El cliente seleccionado no existe o no está activo.");
                return View();
            }

            // Generate a unique 9-digit account number
            var rnd = new Random();
            string accountNumber = rnd.Next(100000000, 999999999).ToString();
            
            var dto = new SavingAccountDto
            {
                Id = 0,
                ClientId = clientId,
                AccountNumber = accountNumber,
                Balance = 0,
                AccountType = SavingAccountType.Secondary,
                Status = SavingAccountStatus.Active
            };
            
            await _savingAccountService.AddAsync(dto);

            TempData["SuccessMessage"] = "Cuenta de ahorro secundaria creada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
