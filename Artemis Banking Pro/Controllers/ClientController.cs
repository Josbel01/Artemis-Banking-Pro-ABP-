using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Artemis_Banking_Pro.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ISavingAccountService _savingAccountService;
        private readonly ILoanService _loanService;
        private readonly ICreditCardService _creditCardService;

        public ClientController(
            ISavingAccountService savingAccountService,
            ILoanService loanService,
            ICreditCardService creditCardService)
        {
            _savingAccountService = savingAccountService;
            _loanService = loanService;
            _creditCardService = creditCardService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Saving accounts
            var accounts = await _savingAccountService.GetAllByClientIdAsync(userId);
            var activeAccounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            ViewBag.TotalBalance = activeAccounts.Sum(a => a.Balance);
            ViewBag.ActiveAccounts = activeAccounts.Count;
            ViewBag.Accounts = activeAccounts;

            // Loans
            var loans = await _loanService.GetAllByClientIdAsync(userId);
            var activeLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
            ViewBag.ActiveLoans = activeLoans.Count;
            ViewBag.Loans = activeLoans;

            // Credit cards
            var cards = await _creditCardService.GetAllByClientIdAsync(userId);
            var activeCards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();
            ViewBag.ActiveCards = activeCards.Count;
            ViewBag.Cards = activeCards;

            // Client info
            var primaryAccount = activeAccounts.FirstOrDefault(a => a.AccountType == SavingAccountType.Main);
            ViewBag.ClientName = User.Identity?.Name ?? "";
            ViewBag.PrimaryAccountNumber = primaryAccount?.AccountNumber ?? "N/A";

            return View();
        }
    }
}
