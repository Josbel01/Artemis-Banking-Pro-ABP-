using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ABP.Core.Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Endpoints for managing users")]
    public class UserController : BaseApiController
    {
        private readonly IAccountServiceWebApi _accountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;
        private readonly ICommerceRepository _commerceRepository;

        public UserController(
            IAccountServiceWebApi accountService, 
            UserManager<AppUser> userManager,
            ISavingAccountService savingAccountService,
            ITransactionService transactionService,
            ICommerceRepository commerceRepository)
        {
            _accountService = accountService;
            _userManager = userManager;
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
            _commerceRepository = commerceRepository;
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

            var users = await _accountService.GetAllUser(null);
            var validUsers = users.Where(u => u.Roles != null && !u.Roles.Contains(UserRoles.Commerce.ToString())).ToList();

            if (!string.IsNullOrWhiteSpace(role))
            {
                validUsers = validUsers.Where(u => u.Roles != null && u.Roles.Contains(role, StringComparer.OrdinalIgnoreCase)).ToList();
            }

            validUsers = validUsers.OrderByDescending(u => u.Id).ToList();

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

            var users = await _accountService.GetAllUser(null);
            var commerceUsers = users.Where(u => u.Roles != null && u.Roles.Contains(UserRoles.Commerce.ToString()))
                                     .OrderByDescending(u => u.Id)
                                     .ToList();

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

            if (dto.Role == UserRoles.Commerce.ToString())
            {
                return BadRequest(new { Message = "No se puede crear un usuario con rol Comercio desde este endpoint." });
            }

            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null) return Conflict(new { Message = "El email ya está registrado." });

            var existingUserName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUserName != null) return Conflict(new { Message = "El nombre de usuario ya está registrado." });

            var users = await _accountService.GetAllUser(null);
            if (users.Any(user => user.DNI == dto.DNI)) return Conflict(new { Message = "La cédula ya está registrada." });

            dto.IsActive = false;
            var response = await _accountService.RegisterUser(dto, Request.Headers["origin"], true);

            if (response.HasError) return BadRequest((response.Errors?.FirstOrDefault() ?? ""));

            if (dto.Role == UserRoles.Client.ToString())
            {
                string accountNumber = await GenerateUniqueAccountNumberAsync();

                var newAccount = new ABP.Core.Application.Dtos.SavingAccounts.SavingAccountDto
                {
                    Id = 0,
                    ClientId = response.Id,
                    AccountNumber = accountNumber,
                    Balance = 0m,
                    AccountType = SavingAccountType.Main,
                    Status = SavingAccountStatus.Active
                };

                var createdAccount = await _savingAccountService.AddAsync(newAccount);

            }

            return StatusCode(201, new { Message = "Usuario creado correctamente.", id = response.Id });
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

            var commerce = await _commerceRepository.GetByIdAsync(commerceId);
            if (commerce == null) return NotFound(new { Message = "El comercio indicado no existe." });

            if (!string.IsNullOrEmpty(commerce.UserId))
            {
                return Conflict(new { Message = "El comercio ya tiene un usuario asociado." });
            }

            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail != null) return Conflict(new { Message = "El email ya está registrado." });

            var existingUserName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUserName != null) return Conflict(new { Message = "El nombre de usuario ya está registrado." });

            dto.Role = UserRoles.Commerce.ToString();
            dto.IsActive = false; 

            var response = await _accountService.RegisterUser(dto, null, true);

            if (response.HasError) return BadRequest((response.Errors?.FirstOrDefault() ?? ""));

            commerce.UserId = response.Id;
            await _commerceRepository.UpdateAsync(commerceId, commerce);

            string accountNumber = await GenerateUniqueAccountNumberAsync();

            var newAccount = new ABP.Core.Application.Dtos.SavingAccounts.SavingAccountDto
            {
                Id = 0,
                ClientId = response.Id,
                AccountNumber = accountNumber,
                Balance = 0m,
                AccountType = SavingAccountType.Main,
                Status = SavingAccountStatus.Active
            };

            var createdAccount = await _savingAccountService.AddAsync(newAccount);


            return StatusCode(201, new { Message = "Usuario comercio creado correctamente.", id = response.Id });
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

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { Message = "El usuario indicado no existe." });

            dto.Id = id;
            dto.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;

            var result = await _accountService.EditUser(dto, null, false, true);

            if (result.HasError) return BadRequest(result.Errors);

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
        public async Task<IActionResult> ChangeUserStatus(string id, [FromBody] ChangeUserStatusDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { Message = "El usuario indicado no existe." });

            user.IsActive = dto.Status;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpGet("users/{id}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get user details")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _accountService.GetUserById(id);
            if (user == null) return NotFound(new { Message = "El usuario indicado no existe." });

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
                createdAt = DateTime.UtcNow 
            });
        }

        private async Task<string> GenerateUniqueAccountNumberAsync()
        {
            var existingNumbers = (await _savingAccountService.GetAllAsync())
                .Select(account => account.AccountNumber)
                .ToHashSet();
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var number = RandomNumberGenerator.GetInt32(100_000_000, 1_000_000_000).ToString();
                if (!existingNumbers.Contains(number)) return number;
            }
            throw new InvalidOperationException("No fue posible generar un número de cuenta único.");
        }
    }

    public class ChangeUserStatusDto
    {
        public bool Status { get; set; }
    }
}







