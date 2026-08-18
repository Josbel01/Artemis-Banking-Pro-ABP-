using ABP.Core.Domain.Interfaces;
using ABP.Core.Domain.Entities;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Commands.CreateCommerce
{
    public class CreateCommerceCommandHandler : IRequestHandler<CreateCommerceCommand, object>
    {
        private readonly ICommerceRepository _commerceRepository;

        public CreateCommerceCommandHandler(ICommerceRepository commerceRepository)
        {
            _commerceRepository = commerceRepository;
        }

        public async Task<object> Handle(CreateCommerceCommand request, CancellationToken cancellationToken)
        {
            var commerces = await _commerceRepository.GetAllListAsync();
            if (commerces.Any(c => c.Rnc == request.Rnc || c.Email == request.Email))
            {
                throw new Exception("Ya existe un comercio con el mismo RNC o correo electronico.");
            }

            var commerce = new Commerce
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Rnc = request.Rnc,
                IsActive = true
            };

            var created = await _commerceRepository.AddAsync(commerce);

            return new
            {
                id = created.Id,
                name = created.Name,
                description = created.Description,
                email = created.Email,
                phoneNumber = created.PhoneNumber,
                rnc = created.Rnc,
                isActive = created.IsActive,
                createdAt = DateTime.Now
            };
        }
    }
}
