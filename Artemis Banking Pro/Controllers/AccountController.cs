using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.User;
using Microsoft.AspNetCore.Mvc;

namespace Artemis_Banking_Pro.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountServiceWebApp _accountService;

        public AccountController(IAccountServiceWebApp accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Admin"); 
                
                if (User.IsInRole("Cashier"))
                    return RedirectToAction("Home", "Cashier");
                
                return RedirectToAction("Index", "Client"); 
            }

            return View("~/Views/Home/Index.cshtml", new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Index.cshtml", model);
            }

            var loginDto = new LoginDto
            {
                UserName = model.Username,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            var response = await _accountService.AuthenticateAsync(loginDto);

            if (response.HasError)
            {
                foreach (var error in response.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View("~/Views/Home/Index.cshtml", model);
            }

            if (response.Roles != null)
            {
                if (response.Roles.Contains("Commerce"))
                {
                    await _accountService.SignOutAsync();
                    return RedirectToAction("AccessDenied");
                }
                
                if (response.Roles.Contains("Admin"))
                    return RedirectToAction("Index", "Admin"); 
                
                if (response.Roles.Contains("Cashier"))
                    return RedirectToAction("Home", "Cashier");
            }

            return RedirectToAction("Index", "Client");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}
