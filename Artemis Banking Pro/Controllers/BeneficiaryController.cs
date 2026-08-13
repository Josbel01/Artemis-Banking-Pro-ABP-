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
        public async Task<IActionResult> Create(SaveBeneficiaryViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var clientId = GetCurrentClientId();
            vm.ClientId = clientId ?? string.Empty;

            var dto = _mapper.Map<ABP.Core.Application.Dtos.Beneficiaries.BeneficiaryDto>(vm);
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
