using ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment;
using ABP.Core.Application.Features.HermesPay.Queries.GetPaymentTransactions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin,Commerce")]
    [SwaggerTag("Provides endpoints for managing Hermes Pay transactions")]
    public class PayController : BaseApiController
    {
        [HttpPost("process-payment/{commerceId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Process a new payment",
            Description = "Processes a credit card payment for a specific commerce. Returns 204 if successful."
        )]
        public async Task<IActionResult> ProcessPayment(int commerceId, [FromBody] ProcessPaymentCommand command)
        {
            if (User.IsInRole("Commerce"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Forbid();
                
                command.CommerceUserId = userId;

            }
            else if (User.IsInRole("Admin"))
            {
                command.CommerceId = commerceId;
                command.CommerceUserId = null;
            }
            else
            {
                return Forbid();
            }

            await Mediator.Send(command);
            return NoContent();
        }

        [HttpGet("get-transactions/{commerceId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get payment transactions",
            Description = "Retrieves a paginated list of Hermes Pay transactions received by a specific commerce."
        )]
        public async Task<IActionResult> GetTransactions(int commerceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = new GetPaymentTransactionsQuery
            {
                Page = page,
                PageSize = pageSize
            };

            if (User.IsInRole("Commerce"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Forbid();
                
                query.CommerceUserId = userId;
            }
            else if (User.IsInRole("Admin"))
            {
                query.CommerceId = commerceId;
                query.CommerceUserId = null;
            }
            else
            {
                return Forbid();
            }

            var response = await Mediator.Send(query);
            return Ok(response);
        }
    }
}
