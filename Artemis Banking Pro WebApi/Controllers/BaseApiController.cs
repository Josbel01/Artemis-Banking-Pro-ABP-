using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
    }
}
