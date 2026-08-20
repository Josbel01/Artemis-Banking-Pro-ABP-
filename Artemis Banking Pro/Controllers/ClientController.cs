using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Artemis_Banking_Pro.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ABP.Core.Application.Interfaces.ISavingAccountService _savingAccountService;

        public ClientController(ABP.Core.Application.Interfaces.ISavingAccountService savingAccountService)
        {
            _savingAccountService = savingAccountService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var accounts = await _savingAccountService.GetAllByClientIdAsync(userId);
            
            ViewBag.TotalBalance = accounts.Sum(a => a.Balance);
            ViewBag.ActiveAccounts = accounts.Count(a => a.Status == ABP.Core.Domain.Common.Enums.SavingAccountStatus.Active);

            return View();
        }
    }
}
