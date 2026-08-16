
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Commands.CreateCommerce
{
    /// <summary>
    /// Parameters required to create a new commerce
    /// </summary>
    public class CreateCommerceCommand : IRequest<int>
    {
        [SwaggerParameter(Description = "The name of the commerce")]
        public string Name { get; set; } = string.Empty;

        [SwaggerParameter(Description = "A brief description of the commerce")]
        public string Description { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The contact email for the commerce")]
        public string Email { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The contact phone number for the commerce")]
        public string PhoneNumber { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The National Taxpayer Registry (RNC) number")]
        public string Rnc { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The associated Identity User ID, if any")]
        public string? UserId { get; set; }
    }

    public class CreateCommerceCommandHandler : IRequestHandler<CreateCommerceCommand, int>
    {
        private readonly ICommerceRepository _commerceRepository;
        private readonly ILogger _logger;

        public CreateCommerceCommandHandler(ICommerceRepository commerceRepository, ILoggerFactory loggerFactory)
        {
            _commerceRepository = commerceRepository;
            _logger = loggerFactory.CreateLogger<CreateCommerceCommandHandler>();
        }

        public async Task<int> Handle(CreateCommerceCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating commerce with Name: {Name}, RNC: {Rnc}", command.Name, command.Rnc);

            var entity = new Commerce
            {
                Name = command.Name,
                Description = command.Description,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                Rnc = command.Rnc,
                UserId = command.UserId
            };

            var result = await _commerceRepository.AddAsync(entity);

            _logger.LogInformation("Commerce creation result: {Result}", result != null ? "Success" : "Failure");
            
            if (result == null)
            {
                throw new Exception("Error creating commerce");
            }

            return result.Id;
        }
    }
}