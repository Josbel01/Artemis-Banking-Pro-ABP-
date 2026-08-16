using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Artemis_Banking_Pro_WebApi.Controllers.v1
{
    /// <summary>
    /// Base controller that provides access to MediatR for all derived API controllers.
    /// </summary>
    /// <remarks>
    /// This controller serves as the base for all versioned API controllers and sets up the MediatR mediator 
    /// through dependency injection using the current HTTP request scope.
    /// </remarks>
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;

        /// <summary>
        /// Provides access to the MediatR mediator instance from the current request services.
        /// </summary>
        protected IMediator Mediator => _mediator ??= HttpContext!.RequestServices.GetService<IMediator>()!;
    }
}
