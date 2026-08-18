using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Commands.UpdateCommerce
{
    public class UpdateCommerceCommandHandler : IRequestHandler<UpdateCommerceCommand, bool>
    {
        private readonly ICommerceRepository _commerceRepository;

        public UpdateCommerceCommandHandler(ICommerceRepository commerceRepository)
        {
            _commerceRepository = commerceRepository;
        }

        public async Task<bool> Handle(UpdateCommerceCommand request, CancellationToken cancellationToken)
        {
            var commerce = await _commerceRepository.GetByIdAsync(request.Id);
            if (commerce == null) return false;

            var commerces = await _commerceRepository.GetAllListAsync();
            if (commerces.Any(c => c.Id != request.Id && (c.Rnc == request.Rnc || c.Email == request.Email)))
            {
                throw new Exception("El RNC o correo electronico pertenece a otro comercio.");
            }

            commerce.Name = request.Name;
            commerce.Description = request.Description ?? commerce.Description;
            commerce.Email = request.Email;
            commerce.PhoneNumber = request.PhoneNumber;
            commerce.Rnc = request.Rnc;

            await _commerceRepository.UpdateAsync(request.Id, commerce);

            return true;
        }
    }
}
