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
        private readonly IBaseAccountService _accountService;
        private readonly ITransactionService _transactionService;

        public AdminController(
            ILoanService loanService,
            ICreditCardService creditCardService,
            ISavingAccountService savingAccountService,
            IBaseAccountService accountService,
            ITransactionService transactionService)
        {
            _loanService = loanService;
            _creditCardService = creditCardService;
            _savingAccountService = savingAccountService;
            _accountService = accountService;
            _transactionService = transactionService;
        }

        public async Task<IActionResult> Index()
        {
            var loans = await _loanService.GetAllAsync();
            var creditCards = await _creditCardService.GetAllAsync();
            var savingAccounts = await _savingAccountService.GetAllAsync();
            var allUsers = await _accountService.GetAllUser();
            var transactions = await _transactionService.GetAllAsync();

            var clients = allUsers.Where(u => u.Roles != null && u.Roles.Contains("Client")).ToList();
            var activeClients = clients.Where(c => c.IsActive).ToList();
            var inactiveClients = clients.Where(c => !c.IsActive).ToList();

            // Debt calculation
            decimal totalLoanDebt = loans
                .Where(l => l.Status == LoanStatus.Active)
                .Sum(l => l.AmountPending);
            decimal totalCardDebt = creditCards
                .Where(c => c.Status == CreditCardStatus.Active)
                .Sum(c => c.CurrentDebt);
            decimal totalDebt = totalLoanDebt + totalCardDebt;

            // Payments = transactions of type Credit that are payments (loan payments, credit card payments)
            var allTransactions = transactions.ToList();
            var today = DateTime.Today;
            var todayTransactions = allTransactions.Count(t => t.TransactionDate.Date == today);

            var vm = new AdminDashboardViewModel
            {
                // Products
                ActiveLoans = loans.Count(l => l.Status == LoanStatus.Active),
                ActiveCreditCards = creditCards.Count(c => c.Status == CreditCardStatus.Active),
                ActiveSavingAccounts = savingAccounts.Count(s => s.Status == SavingAccountStatus.Active),

                // Clients
                ActiveClients = activeClients.Count,
                InactiveClients = inactiveClients.Count,

                // Transactions
                TotalHistoricalTransactions = allTransactions.Count,
                TodayTransactions = todayTransactions,

                // Payments = only CreditCardPayment and LoanPayment (per PDF: deposits, withdrawals, transfers, cash advances are NOT payments)
                TotalHistoricalPayments = allTransactions.Count(t => t.Type == TransactionType.CreditCardPayment || t.Type == TransactionType.LoanPayment),
                TodayPayments = allTransactions.Count(t => (t.Type == TransactionType.CreditCardPayment || t.Type == TransactionType.LoanPayment) && t.TransactionDate.Date == today),

                // Average debt
                AverageDebtPerClient = activeClients.Count > 0 ? totalDebt / activeClients.Count : 0
            };

            return View(vm);
        }
    }
}
