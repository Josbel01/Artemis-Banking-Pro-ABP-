using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.Admin;
using ABP.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Artemis_Banking_Pro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly ICreditCardService _creditCardService;
        private readonly ISavingAccountService _savingAccountService;

        public AdminController(
            ILoanService loanService,
            ICreditCardService creditCardService,
            ISavingAccountService savingAccountService)
        {
            _loanService = loanService;
            _creditCardService = creditCardService;
            _savingAccountService = savingAccountService;
        }

        public async Task<IActionResult> Index()
        {
            var loans = await _loanService.GetAllAsync();
            var creditCards = await _creditCardService.GetAllAsync();
            var savingAccounts = await _savingAccountService.GetAllAsync();

            var vm = new AdminDashboardViewModel
            {
                ActiveLoans = loans.Count(l => l.Status == LoanStatus.Active),
                ActiveCreditCards = creditCards.Count(c => c.Status == CreditCardStatus.Active),
                ActiveSavingAccounts = savingAccounts.Count(s => s.Status == SavingAccountStatus.Active)
            };

            return View(vm);
        }
    }
}
