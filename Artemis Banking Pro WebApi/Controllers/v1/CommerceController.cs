using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Commerces.Queries.GetAllCommerces;
using ABP.Core.Application.Features.Commerces.Queries.GetCommerceById;
using ABP.Core.Application.Features.Commerces.Commands.CreateCommerce;
using ABP.Core.Application.Features.Commerces.Commands.UpdateCommerce;
using ABP.Core.Application.Features.Commerces.Commands.UpdateCommerceStatus;
using System;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Endpoints for managing commerces")]
    public class CommerceController : BaseApiController
    {
        [HttpGet("commerce")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Get commerces")]
        public async Task<IActionResult> GetCommerces([FromQuery] GetAllCommercesQuery query)
        {
            if (query.Page <= 0 || query.PageSize <= 0) return BadRequest("Parametros invalidos");
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("commerce/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get commerce details")]
        public async Task<IActionResult> GetCommerceById(int id)
        {
            var result = await Mediator.Send(new GetCommerceByIdQuery { Id = id });
            if (result == null) return NotFound(new { Message = "El comercio indicado no existe." });
            return Ok(result);
        }

        [HttpPost("commerce")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Create commerce")]
        public async Task<IActionResult> CreateCommerce([FromBody] CreateCommerceCommand command)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await Mediator.Send(command);
                return StatusCode(201, result);
            }
            catch(Exception ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        [HttpPut("commerce/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Update commerce")]
        public async Task<IActionResult> UpdateCommerce(int id, [FromBody] UpdateCommerceCommand command)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            command.Id = id;

            try
            {
                var success = await Mediator.Send(command);
                if (!success) return NotFound(new { Message = "El comercio indicado no existe." });
                return NoContent();
            }
            catch(Exception ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        [HttpPatch("commerce/{id}/status")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Update commerce status")]
        public async Task<IActionResult> UpdateCommerceStatus(int id, [FromBody] UpdateCommerceStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var success = await Mediator.Send(new UpdateCommerceStatusCommand { Id = id, Status = dto.Status });
            if (!success) return NotFound(new { Message = "El comercio indicado no existe." });

            return NoContent();
        }
    }

    public class UpdateCommerceStatusDto
    {
        public bool Status { get; set; }
    }
}
