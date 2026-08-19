using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.CreditCards;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Application.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IBaseAccountService _accountService;
        private readonly IMapper _mapper;

        public CreditCardController(
            ICreditCardService creditCardService, 
            IBaseAccountService accountService,
            IMapper mapper)
        {
            _creditCardService = creditCardService;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? identification, string? status)
        {
            var cards = await _creditCardService.GetAllAsync();
            
            // Get all clients to map names
            var allUsers = await _accountService.GetAllUser();

            // Filter by client identification
            if (!string.IsNullOrEmpty(identification))
            {
                var user = allUsers.FirstOrDefault(u => u.DNI == identification);
                if (user != null)
                {
                    cards = cards.Where(c => c.ClientId == user.Id).ToList();
                }
                else
                {
                    ModelState.AddModelError("", "No existe un cliente registrado con esta cÃ©dula.");
                    cards = new List<CreditCardDto>(); // empty
                }
                ViewBag.Identification = identification;
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status) && status != "Todas")
            {
                if (Enum.TryParse<CreditCardStatus>(status, true, out var cardStatus))
                {
                    cards = cards.Where(c => c.Status == cardStatus).ToList();
                }
                ViewBag.Status = status;
            }
            else
            {
                // Default to active cards
                if (string.IsNullOrEmpty(status))
                {
                    cards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();
                }
                ViewBag.Status = status ?? "Activas";
            }

            // Order by most recent
            cards = cards.OrderByDescending(c => c.Id).ToList();

            var viewModels = _mapper.Map<List<CreditCardViewModel>>(cards);
            
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
            var activeClients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            
            ViewBag.Clients = activeClients;
            return View(new SaveCreditCardViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveCreditCardViewModel vm)
        {
            var allUsers = await _accountService.GetAllUser(isActive: true);
            var activeClients = allUsers.Where(u => u.Roles.Contains("Client")).ToList();
            ViewBag.Clients = activeClients;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var client = await _accountService.GetUserById(vm.ClientId);
            if (client == null || !client.IsActive)
            {
                ModelState.AddModelError("", "El cliente seleccionado no existe o no estÃ¡ activo.");
                return View(vm);
            }

            // Generate a unique 16-digit card number
            var rnd = new Random();
            string cardNumber = $"{rnd.Next(1000, 9999)}{rnd.Next(1000, 9999)}{rnd.Next(1000, 9999)}{rnd.Next(1000, 9999)}";
            
            // Generate CVC
            string cvc = rnd.Next(100, 999).ToString();
            
            // Generate Expiration Date (3 years from now)
            var expirationDate = DateTime.Now.AddYears(3);

            // Map to Dto
            var dto = new CreditCardDto
            {
                Id = 0,
                ClientId = vm.ClientId,
                CreditLimit = vm.CreditLimit,
                CardNumber = cardNumber,
                Cvc = PasswordEncryptation.ComputeSha256Hash(cvc), // Hashed according to the PDF
                ExpirationDate = expirationDate.ToString("MM/yy"),
                CurrentDebt = 0,
                Status = CreditCardStatus.Active,
                AssignedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
            };
            
            // Save the card
            await _creditCardService.AddAsync(dto);

            TempData["SuccessMessage"] = "Tarjeta de crÃ©dito asignada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}

