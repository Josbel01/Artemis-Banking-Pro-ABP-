using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace ABP.Infrastructure.Identity.Services
{
    public class AccountServiceWebApp : BaseAccountService, IAccountServiceWebApp
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountServiceWebApp(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
            : base(userManager, emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto responseDto = new()
            {
                Id = "",
                FirstName = "",
                LastName = "",
                UserName = "",
                Email = "",
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByEmailAsync(loginDto.UserName) ?? await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add($"No existe ninguna cuenta con {loginDto.UserName}");

                return responseDto;
            }

            if (!user.EmailConfirmed)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add($"La cuenta no está activada. Confirme su correo electrónico.");

                return responseDto;
            }

            if (!user.IsActive)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add($"La cuenta no está activada.");

                return responseDto;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            if (roleList.Contains(UserRoles.Commerce.ToString()))
            {
                responseDto.HasError = true;
                responseDto.Errors.Add("No tiene permisos para acceder a la aplicación web.");
                return responseDto;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginDto.Password, loginDto.RememberMe, true);

            if (!result.Succeeded)
            {
                responseDto.HasError = true;

                if (result.IsLockedOut)
                {
                    responseDto.Errors.Add($"La cuenta está bloqueada temporalmente por múltiples intentos fallidos.");
                }
                else
                {
                    responseDto.Errors.Add($"Credenciales inválidas para el usuario {loginDto.UserName}");
                }
                return responseDto;
            }

            responseDto.Id = user.Id;
            responseDto.Email = user.Email ?? "";
            responseDto.UserName = user.UserName ?? "";
            responseDto.FirstName = user.Name ?? "";
            responseDto.LastName = user.LastName ?? "";
            responseDto.IsVerified = user.EmailConfirmed;
            responseDto.Roles = roleList.ToList();

            return responseDto;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
