using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Threading.Tasks;
using ABP.Core.Application.Features.SavingAccounts.Queries.GetAllSavingsAccounts;
using ABP.Core.Application.Features.SavingAccounts.Queries.GetSavingsAccountTransactions;
using ABP.Core.Application.Features.SavingAccounts.Commands.CreateSavingsAccountAPI;
using ABP.Core.Application.Features.SavingAccounts.Commands.DeleteSavingsAccountAPI;
using System;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Endpoints for managing savings accounts")]
    public class SavingsAccountController : BaseApiController
    {
        [HttpGet("savings-account")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Get savings accounts", Description = "Returns a paginated list of savings accounts")]
        public async Task<IActionResult> GetSavingsAccounts([FromQuery] GetAllSavingsAccountsQuery query)
        {
            if (query.Page <= 0 || query.PageSize <= 0) return BadRequest("Parámetros inválidos");

            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("savings-account/{accountNumber}/transactions")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get savings account transactions", Description = "Returns detailed information of a savings account and its transactions")]
        public async Task<IActionResult> GetSavingsAccountTransactions(string accountNumber, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await Mediator.Send(new GetSavingsAccountTransactionsQuery { AccountNumber = accountNumber, Page = page, PageSize = pageSize });
            if (result == null) return NotFound(new { Message = "La cuenta indicada no existe." });
            return Ok(result);
        }

        [HttpPost("savings-account")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Create secondary savings account", Description = "Creates a new secondary savings account for an active client")]
        public async Task<IActionResult> CreateSavingsAccount([FromBody] CreateSavingsAccountAPICommand command)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var result = await Mediator.Send(command);
            return StatusCode(201, result);
        }

        [HttpPatch("savings-account/{accountNumber}/cancel")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Cancel secondary savings account", Description = "Cancels an active secondary savings account")]
        public async Task<IActionResult> CancelSavingsAccount(string accountNumber)
        {
            try 
            {
                var success = await Mediator.Send(new DeleteSavingsAccountAPICommand { AccountNumber = accountNumber });
                if (!success) return BadRequest(new { Message = "La cuenta es principal, ya está cancelada o no puede cancelarse." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
