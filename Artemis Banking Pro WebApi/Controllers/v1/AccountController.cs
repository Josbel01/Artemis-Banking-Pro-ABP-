using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Endpoints for user authentication and account recovery")]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Authenticate user", Description = "Validates user credentials and returns a JWT token")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.Login(loginDto);

            if (response.HasError)
            {
                if ((response.Errors?.FirstOrDefault() ?? "") == "Su cuenta se encuentra inactiva. Debe activar su cuenta antes de iniciar sesión.")
                {
                    return BadRequest(new { Message = (response.Errors?.FirstOrDefault() ?? "") });
                }
                
                if ((response.Errors?.FirstOrDefault() ?? "") == "Acceso denegado. No tiene permisos para utilizar este recurso.")
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { Message = (response.Errors?.FirstOrDefault() ?? "") });
                }

                return Unauthorized(new { Message = (response.Errors?.FirstOrDefault() ?? "") ?? "No tiene autorización para acceder a este recurso." });
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

        [AllowAnonymous]
        [HttpPost("account/confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Confirm user account", Description = "Validates and confirms a user's account using a token")]
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

        [AllowAnonymous]
        [HttpPost("account/get-reset-token")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Get password reset token", Description = "Generates a token to reset the password and sends it via email")]
        public async Task<IActionResult> GetResetToken([FromBody] GetResetTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _accountService.ForgotPasswordAsync(new ForgotPasswordRequestDto
            {
                UserName = request.UserName,
                Origin = Request.Headers["origin"]
            }, true);

            if (response.HasError)
            {
                return BadRequest(new { Message = (response.Errors?.FirstOrDefault() ?? "") });
            }

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("account/reset-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Reset user password", Description = "Resets the user's password using the provided reset token")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordApiRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return BadRequest(new { Message = "El usuario indicado no existe." });
            }

            var dto = new ResetPasswordRequestDto
            {
                Email = user.Email!,
                Token = request.Token,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword
            };

            var response = await _accountService.ResetPasswordAsync(dto);

            if (response.HasError)
            {
                return BadRequest(new { Message = (response.Errors?.FirstOrDefault() ?? "") });
            }

            return NoContent();
        }
    }
}



