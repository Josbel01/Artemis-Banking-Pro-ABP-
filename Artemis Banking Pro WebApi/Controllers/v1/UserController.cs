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
    [Authorize(Roles = "Admin,Commerce")]
    [ApiVersion("1.0")]
    public class UserController : BaseApiController
    {
        private readonly IAccountServiceWebApi _accountService;
        private readonly UserManager<AppUser> _userManager;

        public UserController(IAccountServiceWebApi accountService, UserManager<AppUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        [Authorize(Roles = nameof(UserRoles.Admin))]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var users = await _accountService.GetAllUser();
                
                var allowedRoles = new List<string> { 
                    UserRoles.Admin.ToString(), 
                    UserRoles.Cashier.ToString(), 
                    UserRoles.Client.ToString() 
                };

                var validUsers = users.Where(u => u.Roles != null && u.Roles.Any(r => allowedRoles.Contains(r))).ToList();

                return Ok(validUsers);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
