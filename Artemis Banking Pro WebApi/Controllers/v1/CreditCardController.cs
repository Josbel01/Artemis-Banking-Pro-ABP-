using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Threading.Tasks;
using ABP.Core.Application.Features.CreditCards.Queries.GetAllCreditCards;
using ABP.Core.Application.Features.CreditCards.Queries.GetCreditCardById;
using ABP.Core.Application.Features.CreditCards.Commands.CreateCreditCardAPI;
using ABP.Core.Application.Features.CreditCards.Commands.UpdateCreditCardLimit;
using ABP.Core.Application.Features.CreditCards.Commands.DeleteCreditCardAPI;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Endpoints for managing credit cards")]
    public class CreditCardController : BaseApiController
    {
        [HttpGet("credit-card")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Get credit cards", Description = "Returns a paginated list of credit cards")]
        public async Task<IActionResult> GetCreditCards([FromQuery] GetAllCreditCardsQuery query)
        {
            if (query.Page <= 0 || query.PageSize <= 0) return BadRequest("Parámetros inválidos");

            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("credit-card/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get credit card details", Description = "Returns detailed information of a credit card and its consumptions")]
        public async Task<IActionResult> GetCreditCardById(string id)
        {
            var result = await Mediator.Send(new GetCreditCardByIdQuery { Id = id });
            if (result == null) return NotFound(new { Message = "La tarjeta indicada no existe." });
            return Ok(result);
        }

        [HttpPost("credit-card")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Assign credit card to client", Description = "Creates a new credit card")]
        public async Task<IActionResult> CreateCreditCard([FromBody] CreateCreditCardAPICommand command)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await Mediator.Send(command);
            return StatusCode(201, result);
        }

        [HttpPatch("credit-card/{id}/limit")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update credit card limit", Description = "Updates the credit limit of an active credit card")]
        public async Task<IActionResult> UpdateCreditCardLimit(string id, [FromBody] UpdateCreditCardLimitCommand command)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            command.Id = id;
            var success = await Mediator.Send(command);
            if (!success) return NotFound(new { Message = "La tarjeta indicada no existe." });

            return NoContent();
        }

        [HttpPatch("credit-card/{id}/cancel")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Cancel credit card", Description = "Cancels an active credit card if it has no debt")]
        public async Task<IActionResult> CancelCreditCard(string id)
        {
            var success = await Mediator.Send(new DeleteCreditCardAPICommand { Id = id });
            if (!success) return NotFound(new { Message = "La tarjeta indicada no existe." });

            return NoContent();
        }
    }
}
