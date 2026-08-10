using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Beneficiaries;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using ABP.Core.Application.Dtos.Beneficiaries;

namespace ArtemisBankingPro.Controllers
{
    public class BeneficiaryController : Controller
    {
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IMapper _mapper;

        public BeneficiaryController(IBeneficiaryService beneficiaryService, IMapper mapper)
        {
            _beneficiaryService = beneficiaryService;
            _mapper = mapper;
        }

        // Simula o obtiene el ID del usuario actual (Temporal hasta tener Identity)
        private string GetCurrentClientId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "client-mock-123";
        }

        public async Task<IActionResult> Index()
        {
            var clientId = GetCurrentClientId();
            var beneficiaries = await _beneficiaryService.GetAllByClientIdAsync(clientId);
            return View(beneficiaries);
        }

        public IActionResult Create()
        {
            return View(new SaveBeneficiaryViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveBeneficiaryViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var clientId = GetCurrentClientId();
            vm.ClientId = clientId;

            // TODO: Aqui idealmente se verificaria si la cuenta existe usando ISavingAccountService, pero por ahora lo guardamos
            var dto = _mapper.Map<BeneficiaryDto>(vm);
            await _beneficiaryService.AddAsync(dto);

            TempData["SuccessMessage"] = "Beneficiario agregado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _beneficiaryService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Beneficiario eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
