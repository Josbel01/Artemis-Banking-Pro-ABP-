using ABP.Core.Application.Dtos.User;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MediatR;
using ABP.Core.Application.Features.Account.Commands.Login;
using ABP.Core.Application.Features.Account.Commands.ConfirmAccount;
using ABP.Core.Application.Features.Account.Commands.ForgotPassword;
using ABP.Core.Application.Features.Account.Commands.ResetPassword;
using System.Linq;
using System.Threading.Tasks;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Endpoints for user authentication and account recovery")]
    public class AccountController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AccountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("account/login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JwtResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Authenticate user", Description = "Validates user credentials and returns a JWT token")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _mediator.Send(command);

            if (response.HasError)
            {
                if ((response.Errors?.FirstOrDefault() ?? "").Contains("inactiva", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Message = (response.Errors?.FirstOrDefault() ?? "") });
                }
                
                if ((response.Errors?.FirstOrDefault() ?? "").Contains("permisos", StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { Message = (response.Errors?.FirstOrDefault() ?? "") });
                }

                return Unauthorized(new { Message = (response.Errors?.FirstOrDefault() ?? "Credenciales inválidas.") });
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
        public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _mediator.Send(command);

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
        public async Task<IActionResult> GetResetToken([FromBody] ForgotPasswordCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            command.Origin = Request.Headers["origin"];

            var response = await _mediator.Send(command);

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
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _mediator.Send(command);

            if (response.HasError)
            {
                return BadRequest(new { Message = (response.Errors?.FirstOrDefault() ?? "") });
            }

            return NoContent();
        }
    }
}
