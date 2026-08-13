using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.SavingAccounts;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize]
    public class SavingAccountController : Controller
    {
        private readonly ISavingAccountService _savingAccountService;
        private readonly IMapper _mapper;

        public SavingAccountController(ISavingAccountService savingAccountService, IMapper mapper)
        {
            _savingAccountService = savingAccountService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _savingAccountService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<SavingAccountViewModel>>(accounts);
            return View(viewModels);
        }
    }
}
