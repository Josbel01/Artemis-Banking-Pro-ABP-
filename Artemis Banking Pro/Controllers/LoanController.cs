using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Loans;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IMapper _mapper;

        public LoanController(ILoanService loanService, IMapper mapper)
        {
            _loanService = loanService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var loans = await _loanService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<LoanViewModel>>(loans);
            return View(viewModels);
        }
    }
}
