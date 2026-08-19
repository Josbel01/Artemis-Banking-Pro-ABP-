using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Settings;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ABP.Infrastructure.Identity.Services
{
    public class AccountServiceWebApi : BaseAccountService, IAccountServiceWebApi
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AccountServiceWebApi> _logger;

        public AccountServiceWebApi(UserManager<AppUser> userManager, IEmailService emailService, IOptions<JwtSettings> jwtSettings, ILoggerFactory loggerFactory) 
            : base(userManager, emailService, loggerFactory.CreateLogger<AccountServiceWebApi>())
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _logger = loggerFactory.CreateLogger<AccountServiceWebApi>();
        }

        public async Task<LoginResponseApiDto> Login(LoginDto loginDto)
        {
            _logger.LogInformation("Authenticating user {UserName} for API access", loginDto.UserName);
            
            LoginResponseApiDto responseDto = new()
            {
                Name = "",
                LastName = "",
                UserName = "",
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
                responseDto.Errors.Add($"La cuenta está inactiva.");
                return responseDto;
            }

            _logger.LogInformation("Attempting to verify password for user {UserName}", loginDto.UserName);
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if(!result)
            {
                _logger.LogWarning("Invalid credentials for user: {UserName}", loginDto.UserName);
                responseDto.HasError= true;
                responseDto.Errors.Add($"Credenciales inválidas para el usuario {loginDto.UserName}");
                return responseDto;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            if (!roleList.Contains(UserRoles.Admin.ToString()) && !roleList.Contains(UserRoles.Commerce.ToString()))
            {
                _logger.LogWarning("User {UserName} does not have required permissions for Web API. Roles: {Roles}", loginDto.UserName, string.Join(",", roleList));
                responseDto.HasError = true;
                responseDto.Errors.Add("No tiene permisos para acceder a la API web.");
                return responseDto;
            }

            _logger.LogInformation("User {UserName} authenticated successfully. Generating JWT Token.", loginDto.UserName);
            JwtSecurityToken jwtToken = await GenerateJwtToken(user);

            responseDto.Name = user.Name;
            responseDto.LastName = user.LastName;
            responseDto.UserName = user.UserName ?? "";
            responseDto.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            responseDto.Roles = roleList.ToList();
            responseDto.Expiration = jwtToken.ValidTo;

            return responseDto;
        }

        public override async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto requestDto)
        {
            return await base.ResetPasswordAsync(requestDto);
        }

        public override async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            return await base.ConfirmAccountAsync(userId, token);
        }

        #region Private Methods

        private async Task<JwtSecurityToken> GenerateJwtToken(AppUser user)
        {
            var roleList = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            var roleClaims = new List<Claim>();

            foreach (var role in roleList)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var issuedAt = DateTime.UtcNow;
            var expiration = issuedAt.AddMinutes(_jwtSettings.DurationInMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }.Union(userClaims)
             .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: issuedAt,
                expires: expiration,
                signingCredentials: signingCredentials
                );

            return jwtSecurityToken;
        }

        #endregion
    }
}
