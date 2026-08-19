using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ABP.Infrastructure.Identity.Services
{
    public class AccountServiceWebApp : BaseAccountService, IAccountServiceWebApp
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountServiceWebApp> _logger;

        public AccountServiceWebApp(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, ILoggerFactory loggerFactory)
            : base(userManager, emailService, loggerFactory.CreateLogger<AccountServiceWebApp>())
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<AccountServiceWebApp>();
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            _logger.LogInformation("Authenticating user {UserName} for web app access", loginDto.UserName);

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

            _logger.LogInformation("Attempting to find user by username or email: {UserName}", loginDto.UserName);
            var user = await _userManager.FindByEmailAsync(loginDto.UserName) ?? await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                _logger.LogWarning("No account registered with username: {UserName}", loginDto.UserName);
                responseDto.HasError = true;
                responseDto.Errors.Add($"No existe ninguna cuenta con {loginDto.UserName}");

                return responseDto;
            }

            _logger.LogInformation("User found: {UserName} - EmailConfirmed: {EmailConfirmed}", user.UserName, user.EmailConfirmed);

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Account {UserName} is not active, email confirmation required", loginDto.UserName);
                responseDto.HasError = true;
                responseDto.Errors.Add($"La cuenta no está activada. Confirme su correo electrónico.");

                return responseDto;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Account {UserName} is inactive", loginDto.UserName);
                responseDto.HasError = true;
                responseDto.Errors.Add($"La cuenta no está activada.");

                return responseDto;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            if (roleList.Contains(UserRoles.Commerce.ToString()))
            {
                _logger.LogWarning("User {UserName} does not have required permissions for Web App. Roles: {Roles}", loginDto.UserName, string.Join(",", roleList));
                responseDto.HasError = true;
                responseDto.Errors.Add("No tiene permisos para acceder a la aplicación web.");
                return responseDto;
            }

            _logger.LogInformation("Attempting to sign in user: {UserName}", user.UserName);
            var result = await _signInManager.PasswordSignInAsync(user.UserName!, loginDto.Password, loginDto.RememberMe, true);

            _logger.LogInformation("Sign in result for user {UserName}: {Result}", user.UserName, result.Succeeded);
            if (!result.Succeeded)
            {
                responseDto.HasError = true;

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User {UserName} is locked out due to multiple failed attempts", loginDto.UserName);
                    responseDto.Errors.Add($"La cuenta está bloqueada temporalmente por múltiples intentos fallidos.");
                }
                else
                {
                    _logger.LogWarning("Invalid credentials for user: {UserName}", loginDto.UserName);
                    responseDto.Errors.Add($"Credenciales inválidas para el usuario {loginDto.UserName}");
                }
                return responseDto;
            }

            _logger.LogInformation("User {UserName} authenticated successfully.", loginDto.UserName);
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
