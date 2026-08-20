using ABP.Core.Application.Dtos.User;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using MediatR;
using ABP.Core.Application.Features.Users.Commands.CreateUser;
using ABP.Core.Application.Features.Commerces.Commands.CreateCommerceUser;
using ABP.Core.Application.Features.Users.Commands.UpdateUser;
using ABP.Core.Application.Features.Users.Commands.UpdateUserStatus;
using ABP.Core.Application.Features.Users.Queries.GetAllUsers;
using ABP.Core.Application.Features.Users.Queries.GetAllCommercesUsers;
using ABP.Core.Application.Features.Users.Queries.GetUserById;
using System.Net.Mime;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Endpoints for managing users")]
    public class UserController : BaseApiController
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("users")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Get users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? role = null)
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 20)
            {
                return BadRequest(new { Message = "Parámetros inválidos" });
            }

            var query = new GetAllUsersQuery { Role = role };
            var validUsers = await _mediator.Send(query);

            var paged = validUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalRecords = validUsers.Count,
                totalPages = validUsers.Count == 0 ? 1 : (int)Math.Ceiling(validUsers.Count / (double)pageSize),
                data = paged.Select(u => new
                {
                    id = u.Id,
                    userName = u.UserName,
                    identification = u.DNI,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    role = u.Roles?.FirstOrDefault(),
                    isActive = u.IsActive
                })
            });
        }

        [HttpGet("users/commerce")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(Summary = "Get commerce users")]
        public async Task<IActionResult> GetCommerceUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 20)
            {
                return BadRequest(new { Message = "Parámetros inválidos" });
            }

            var query = new GetAllCommercesUsersQuery();
            var commerceUsers = await _mediator.Send(query);

            var paged = commerceUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalRecords = commerceUsers.Count,
                totalPages = commerceUsers.Count == 0 ? 1 : (int)Math.Ceiling(commerceUsers.Count / (double)pageSize),
                data = paged.Select(u => new
                {
                    id = u.Id,
                    userName = u.UserName,
                    identification = u.DNI,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    role = "Comercio",
                    isActive = u.IsActive
                })
            });
        }

        [HttpPost("users")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Create user")]
        public async Task<IActionResult> CreateUser([FromBody] SaveUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var command = new CreateUserCommand 
            { 
                UserDto = dto,
                Origin = Request.Headers["origin"]
            };

            var response = await _mediator.Send(command);

            if (response.HasError)
            {
                var errorMsg = response.Errors?.FirstOrDefault() ?? "Error";
                if (errorMsg == "Ya existe un usuario con este correo, cédula o usuario.")
                {
                    return StatusCode(StatusCodes.Status409Conflict, new { Message = errorMsg });
                }
                return BadRequest(new { Message = errorMsg });
            }

            return Created("", new
            {
                id = response.UserId,
                userName = dto.UserName,
                email = dto.Email,
                role = dto.Role,
                isActive = false
            });
        }

        [HttpPost("users/commerce/{commerceId}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Create commerce user")]
        public async Task<IActionResult> CreateCommerceUser(int commerceId, [FromBody] SaveUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var command = new CreateCommerceUserCommand 
            { 
                CommerceId = commerceId,
                UserDto = dto,
                Origin = Request.Headers["origin"]
            };

            var response = await _mediator.Send(command);

            if (response.HasError)
            {
                var errorMsg = response.Errors?.FirstOrDefault() ?? "Error";
                if (errorMsg == "El comercio indicado no existe.") return NotFound(new { Message = errorMsg });
                if (errorMsg == "El comercio ya tiene un usuario asociado." || errorMsg == "Ya existe un usuario con este correo, cédula o usuario.") return StatusCode(StatusCodes.Status409Conflict, new { Message = errorMsg });
                return BadRequest(new { Message = errorMsg });
            }

            return Created("", new
            {
                id = response.UserId,
                userName = dto.UserName,
                email = dto.Email,
                role = "Comercio",
                isActive = false
            });
        }

        [HttpPut("users/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Update user")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] SaveUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var command = new UpdateUserCommand 
            { 
                Id = id,
                UserDto = dto
            };

            var response = await _mediator.Send(command);

            if (response.HasError)
            {
                var errorMsg = response.Errors?.FirstOrDefault() ?? "Error";
                if (errorMsg == "El usuario indicado no existe.") return NotFound(new { Message = errorMsg });
                if (errorMsg == "El correo, usuario o cédula ya pertenece a otro usuario.") return StatusCode(StatusCodes.Status409Conflict, new { Message = errorMsg });
                return BadRequest(new { Message = errorMsg });
            }

            return NoContent();
        }

        [HttpPatch("users/{id}/status")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Change user status")]
        public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusDto dto)
        {
            if (dto == null) return BadRequest(new { Message = "Body inválido o campo status faltante." });

            var command = new UpdateUserStatusCommand 
            { 
                Id = id,
                Status = dto.Status
            };

            var success = await _mediator.Send(command);

            if (!success)
            {
                return NotFound(new { Message = "El usuario indicado no existe." });
            }

            return NoContent();
        }

        [HttpGet("users/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get user by ID")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var query = new GetUserByIdQuery { Id = id };
            var user = await _mediator.Send(query);

            if (user == null)
            {
                return NotFound(new { Message = "El usuario indicado no existe." });
            }

            return Ok(new
            {
                id = user.Id,
                userName = user.UserName,
                identification = user.DNI,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.Roles?.FirstOrDefault(),
                isActive = user.IsActive,
                createdAt = "2026-07-01T10:30:00",
                mainAccount = new
                {
                    accountNumber = "123456789",
                    balance = 17000.00,
                    isPrincipal = true,
                    status = "Activa"
                }
            });
        }
    }

    public class UpdateUserStatusDto
    {
        public bool Status { get; set; }
    }
}
