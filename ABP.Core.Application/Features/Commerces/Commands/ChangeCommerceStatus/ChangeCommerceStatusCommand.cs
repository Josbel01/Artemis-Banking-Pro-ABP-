using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Commands.ChangeCommerceStatus
{
    public class ChangeCommerceStatusCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChangeCommerceStatusCommandHandler : IRequestHandler<ChangeCommerceStatusCommand, Unit>
    {
        private readonly ICommerceRepository _repository;

        public ChangeCommerceStatusCommandHandler(ICommerceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(ChangeCommerceStatusCommand command, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(command.Id);
            if (entity == null) throw new Exception("Commerce not found");

            entity.IsActive = command.IsActive;

            await _repository.UpdateAsync(command.Id, entity);
            return Unit.Value;
        }
    }
}
