using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.CreditCards;
using ABP.Core.Application.ViewModels.Common;
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
        private readonly ILoanService _loanService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreditCardController(
            ICreditCardService creditCardService, 
            IBaseAccountService accountService,
            ILoanService loanService,
            IEmailService emailService,
            IMapper mapper)
        {
            _creditCardService = creditCardService;
            _accountService = accountService;
            _loanService = loanService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string? identification, string? status, int page = 1)
        {
            int pageSize = 20;
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
                    ModelState.AddModelError("", "No existe un cliente registrado con esta c\u00e9dula.");
                    cards = new List<CreditCardDto>();
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
                if (string.IsNullOrEmpty(status))
                {
                    cards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();
                    ViewBag.Status = "Active";
                }
                else
                {
                    ViewBag.Status = status;
                }
            }

            // Order by most recent
            cards = cards.OrderByDescending(c => c.Id).ToList();

            // Pagination
            int totalRecords = cards.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            var pagedCards = cards.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModels = _mapper.Map<List<CreditCardViewModel>>(pagedCards);
            
            // Map client names
            foreach (var vm in viewModels)
            {
                var client = allUsers.FirstOrDefault(u => u.Id == vm.ClientId);
                if (client != null)
                {
                    vm.ClientName = $"{client.FirstName} {client.LastName}";
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
            
            // Calculate total debt per client
            var activeLoans = await _loanService.GetAllAsync();
            var allCreditCards = await _creditCardService.GetAllAsync();
            
            // Average Debt Calculation
            decimal totalLoanDebt = activeLoans.Where(l => l.Status == LoanStatus.Active).Sum(l => l.AmountPending);
            decimal totalCreditCardDebt = allCreditCards.Sum(c => c.CurrentDebt);
            decimal totalDebt = totalLoanDebt + totalCreditCardDebt;
            int activeClientsCount = activeClients.Count;
            ViewBag.AverageDebt = activeClientsCount > 0 ? totalDebt / activeClientsCount : 0;

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

            ViewBag.ClientName = $"{client.FirstName} {client.LastName}";
            ViewBag.ClientDNI = client.DNI;
            ViewBag.ClientEmail = client.Email;
            ViewBag.ClientId = client.Id;

            // Calculate client total debt
            var activeLoans = await _loanService.GetAllAsync();
            var allCards = await _creditCardService.GetAllAsync();
            ViewBag.ClientDebt = CalculateClientDebt(clientId, activeLoans, allCards);

            return View(new SaveCreditCardViewModel { ClientId = clientId });
        }

        // STEP 2 POST: Process the assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(SaveCreditCardViewModel vm)
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
                Cvc = PasswordEncryptation.ComputeSha256Hash(cvc),
                ExpirationDate = expirationDate,
                CurrentDebt = 0,
                Status = CreditCardStatus.Active,
                AssignedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""
            };
            
            // Save the card
            await _creditCardService.AddAsync(dto);

            // Send email notification per PDF spec: last 4 digits, limit, expiration, date
            var lastFourDigits = cardNumber.Substring(cardNumber.Length - 4);
            var emailBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#1a56db,#3b82f6);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128179; Nueva Tarjeta de Cr&#233;dito</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{client.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Se le ha asignado una nueva tarjeta de cr&#233;dito. A continuaci&#243;n los detalles:</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Tarjeta</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">&#128179; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; &#9679;&#9679;&#9679;&#9679; {lastFourDigits}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">L&#237;mite aprobado</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#16a34a;">RD${vm.CreditLimit:N2}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Expiraci&#243;n</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{expirationDate:MM/yy}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha de asignaci&#243;n</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{DateTime.Now:dd/MM/yyyy}</td></tr>
</table>
<div style="background:#fef9c3;border-left:4px solid #ca8a04;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#854d0e;font-size:13px;margin:0;">&#9888;&#65039; Por seguridad, no comparta la informaci&#243;n de su tarjeta con terceros.</p>
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
                Subject = "Nueva tarjeta de crédito asignada",
                HtmlBody = emailBody
            });

            TempData["SuccessMessage"] = "Tarjeta de cr\u00e9dito asignada correctamente.";
            return RedirectToAction("Index");
        }

        // ==================== Edit Limit ====================
        [HttpGet]
        public async Task<IActionResult> EditLimit(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null)
            {
                TempData["ErrorMessage"] = "La tarjeta seleccionada no existe.";
                return RedirectToAction("Index");
            }

            var client = await _accountService.GetUserById(card.ClientId);
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "";
            ViewBag.CardLastFour = card.CardNumber.Length >= 4 ? card.CardNumber.Substring(card.CardNumber.Length - 4) : card.CardNumber;
            ViewBag.CurrentDebt = card.CurrentDebt;

            var vm = new UpdateCreditCardLimitViewModel
            {
                Id = card.Id,
                CreditLimit = card.CreditLimit
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLimit(UpdateCreditCardLimitViewModel vm)
        {
            var card = await _creditCardService.GetByIdAsync(vm.Id);
            if (card == null)
            {
                TempData["ErrorMessage"] = "La tarjeta seleccionada no existe.";
                return RedirectToAction("Index");
            }

            var client = await _accountService.GetUserById(card.ClientId);
            ViewBag.ClientName = client != null ? $"{client.FirstName} {client.LastName}" : "";
            ViewBag.CardLastFour = card.CardNumber.Length >= 4 ? card.CardNumber.Substring(card.CardNumber.Length - 4) : card.CardNumber;
            ViewBag.CurrentDebt = card.CurrentDebt;

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            if (vm.CreditLimit <= 0)
            {
                ModelState.AddModelError("", "El límite de la tarjeta debe ser mayor que cero.");
                return View(vm);
            }

            if (vm.CreditLimit < card.CurrentDebt)
            {
                ModelState.AddModelError("", "El límite de la tarjeta no puede ser inferior al monto adeudado actualmente.");
                return View(vm);
            }

            card.CreditLimit = vm.CreditLimit;
            await _creditCardService.UpdateAsync(card, card.Id);

            // Send email to client
            if (client != null)
            {
                var lastFour = card.CardNumber.Substring(card.CardNumber.Length - 4);
                var emailBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#1a56db,#3b82f6);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128179; Modificaci&#243;n de L&#237;mite</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{client.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">El l&#237;mite de su tarjeta terminada en <strong>&#9679;&#9679;&#9679;&#9679; {lastFour}</strong> ha sido actualizado.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Nuevo l&#237;mite</td><td style="padding:10px 14px;background:#f8fafc;font-size:18px;font-weight:700;color:#16a34a;border-radius:0 6px 6px 0;">RD${vm.CreditLimit:N2}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Fecha</td><td style="padding:10px 14px;font-size:15px;color:#0b1f3a;">{DateTime.Now:dd/MM/yyyy}</td></tr>
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

                await _emailService.SendAsync(new ABP.Core.Application.Dtos.Email.EmailRequestDto
                {
                    To = client.Email,
                    Subject = "Modificación de límite de tarjeta",
                    HtmlBody = emailBody
                });
            }

            TempData["SuccessMessage"] = "Límite de crédito actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null)
            {
                TempData["ErrorMessage"] = "La tarjeta no existe.";
                return RedirectToAction("Index");
            }

            if (card.CurrentDebt > 0)
            {
                TempData["ErrorMessage"] = "Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.";
                return RedirectToAction("Index");
            }

            card.Status = ABP.Core.Domain.Common.Enums.CreditCardStatus.Cancelled;
            await _creditCardService.UpdateAsync(card, card.Id);

            TempData["SuccessMessage"] = "Tarjeta cancelada correctamente.";
            return RedirectToAction("Index");
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
