using ABP.Core.Domain.Interfaces;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Commands.UpdateCommerce
{
    public class UpdateCommerceCommand : IRequest<Unit>
    {
        [SwaggerParameter(Description = "ID")]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Rnc { get; set; } = string.Empty;
    }

    public class UpdateCommerceCommandHandler : IRequestHandler<UpdateCommerceCommand, Unit>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.Commerce> _repository;

        public UpdateCommerceCommandHandler(IGenericRepository<ABP.Core.Domain.Entities.Commerce> repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateCommerceCommand command, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(command.Id);
            if (entity == null) throw new Exception("Commerce not found");

            entity.Name = command.Name;
            entity.Description = command.Description;
            entity.Email = command.Email;
            entity.PhoneNumber = command.PhoneNumber;
            entity.Rnc = command.Rnc;

            await _repository.UpdateAsync(command.Id, entity);
            return Unit.Value;
        }
    }
}
