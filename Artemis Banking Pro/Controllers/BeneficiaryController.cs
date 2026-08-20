using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Beneficiaries;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArtemisBankingPro.Controllers
{
    [Authorize(Roles = "Client")]
    public class BeneficiaryController : Controller
    {
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly IMapper _mapper;

        public BeneficiaryController(IBeneficiaryService beneficiaryService, IMapper mapper)
        {
            _beneficiaryService = beneficiaryService;
            _mapper = mapper;
        }

        private string? GetCurrentClientId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public async Task<IActionResult> Index()
        {
            var clientId = GetCurrentClientId();
            var dtos = await _beneficiaryService.GetAllByClientIdAsync(clientId ?? string.Empty);
            var viewModels = _mapper.Map<IEnumerable<BeneficiaryViewModel>>(dtos);
            return View(viewModels);
        }

        public IActionResult Create()
        {
            return View(new SaveBeneficiaryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveBeneficiaryViewModel vm)
        {
            try
            {
                var clientId = GetCurrentClientId();
                vm.ClientId = clientId ?? string.Empty;

                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                var dto = _mapper.Map<ABP.Core.Application.Dtos.Beneficiaries.BeneficiaryDto>(vm);
                var result = await _beneficiaryService.AddAsync(dto);

                if (result == null)
                {
                    ModelState.AddModelError("", "No se pudo guardar el beneficiario. Verifique los datos e intente de nuevo.");
                    return View(vm);
                }

                TempData["SuccessMessage"] = "Beneficiario agregado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar el beneficiario: " + ex.Message);
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _beneficiaryService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Beneficiario eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
