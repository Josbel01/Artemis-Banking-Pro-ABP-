using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Settings;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
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

        public AccountServiceWebApi(UserManager<AppUser> userManager, IEmailService emailService, IOptions<JwtSettings> jwtSettings) 
            : base(userManager, emailService)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponseApiDto> Login(LoginDto loginDto)
        {
            LoginResponseApiDto responseDto = new()
            {
                Name = "",
                LastName = "",
                UserName = "",
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
                responseDto.Errors.Add($"La cuenta está inactiva.");
                return responseDto;
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if(!result)
            {
                responseDto.HasError= true;
                responseDto.Errors.Add($"Credenciales inválidas para el usuario {loginDto.UserName}");
                return responseDto;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            if (!roleList.Contains(UserRoles.Admin.ToString()) && !roleList.Contains(UserRoles.Commerce.ToString()))
            {
                responseDto.HasError = true;
                responseDto.Errors.Add("No tiene permisos para acceder a la API web.");
                return responseDto;
            }

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
