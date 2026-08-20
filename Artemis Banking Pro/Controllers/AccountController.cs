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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var origin = $"{Request.Scheme}://{Request.Host}";
            var request = new ForgotPasswordRequestDto
            {
                UserName = model.UserName,
                Origin = origin
            };

            var response = await _accountService.ForgotPasswordAsync(request, false);
            if (response.HasError)
            {
                foreach(var error in response.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(model);
            }

            ViewBag.Message = "Se ha enviado un correo con las instrucciones para restablecer su contraseña.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new ResetPasswordRequestDto
            {
                Email = model.Email,
                Token = model.Token,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword
            };

            var response = await _accountService.ResetPasswordAsync(request);
            if (response.HasError)
            {
                foreach (var error in response.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return View(model);
            }

            return RedirectToAction("Index", new { message = "Contraseña restablecida exitosamente." });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var response = await _accountService.ConfirmAccountAsync(userId, token);
            if (response.HasError)
            {
                ViewBag.Error = response.Message;
                return View();
            }
            // Cerrar sesión anterior para que el nuevo usuario pueda iniciar sesión limpio
            await _accountService.SignOutAsync();
            ViewBag.Message = response.Message;
            return View();
        }
    }
}

