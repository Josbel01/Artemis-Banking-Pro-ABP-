using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AccountController : BaseApiController
    {
        private readonly IAccountServiceWebApi _accountService;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(IAccountServiceWebApi accountService, UserManager<AppUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        [HttpPost("account/login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JwtResponseDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Los datos enviados no son válidos.");
                }

                var response = await _accountService.Login(loginDto);

                if (response.HasError)
                {
                    return Unauthorized(response.Errors);
                }

                return Ok(new JwtResponseDto
                {
                    Token = response.AccessToken,
                    User = response.UserName,
                    Roles = response.Roles ?? [],
                    Expiration = response.Expiration,
                    HasError = false
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("account/confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _accountService.ConfirmAccountAsync(request.UserId, request.Token);

            if (response.HasError)
            {
                return BadRequest(response);
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/commerce")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterTrade([FromBody] SaveUserDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if(registerDto.Password != registerDto.ConfirmPassword)
                {
                    return BadRequest(new { Error = "Password y la confirmacion del password no coinciden " });
                }

                registerDto.Role = UserRoles.Commerce.ToString();
                registerDto.IsActive = true;

                var result = await _accountService.RegisterUser(registerDto, null, true);

                if (result.HasError)
                {
                    return BadRequest(result.Errors);
                }

                var user = await _userManager.FindByIdAsync(result.Id);
                if(user != null)
                {
                    user.EmailConfirmed = true;
                    user.IsActive = true;
                    await _userManager.UpdateAsync(user);
                }

                return StatusCode(StatusCodes.Status201Created, new { Message = "Usuario comercio creado correctamente.", Id = result.Id });

            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
