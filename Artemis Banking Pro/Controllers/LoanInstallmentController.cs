using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Loans;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize]
    public class LoanInstallmentController : Controller
    {
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly IMapper _mapper;

        public LoanInstallmentController(ILoanInstallmentService loanInstallmentService, IMapper mapper)
        {
            _loanInstallmentService = loanInstallmentService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int loanId)
        {
            if (loanId == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var installments = await _loanInstallmentService.GetAllByLoanIdAsync(loanId);
            var viewModels = _mapper.Map<IEnumerable<LoanInstallmentViewModel>>(installments);
            ViewBag.LoanId = loanId;
            return View(viewModels);
        }
    }
}
