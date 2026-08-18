using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Loans.Queries.GetAllLoans;
using ABP.Core.Application.Features.Loans.Queries.GetLoanById;
using ABP.Core.Application.Features.Loans.Commands.CreateLoan;
using ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Endpoints for managing loans")]
    public class LoanController : BaseApiController
    {
        [HttpGet("loan")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Get loans")]
        public async Task<IActionResult> GetLoans([FromQuery] GetAllLoansQuery query)
        {
            if (query.PageNumber <= 0 || query.PageSize <= 0) return BadRequest("Parametros invalidos");
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("loan/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get loan details")]
        public async Task<IActionResult> GetLoanById(string id)
        {
            var result = await Mediator.Send(new GetLoanByIdQuery { Id = id });
            if (result == null) return NotFound(new { Message = "El prestamo indicado no existe." });
            return Ok(result);
        }

        [HttpPost("loan")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Assign loan to client")]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanCommand command)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await Mediator.Send(command);
            return StatusCode(201, result);
        }

        [HttpPatch("loan/{id}/rate")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update loan interest rate")]
        public async Task<IActionResult> UpdateLoanRate(string id, [FromBody] UpdateLoanRateCommand command)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            command.Id = id;
            var success = await Mediator.Send(command);
            if (!success) return NotFound(new { Message = "El prestamo indicado no existe." });
            return NoContent();
        }
    }
}
