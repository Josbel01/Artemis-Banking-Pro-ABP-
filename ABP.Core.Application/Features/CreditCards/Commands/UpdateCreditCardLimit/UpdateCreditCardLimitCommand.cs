using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CreditCards.Commands.UpdateCreditCardLimit
{
    public class UpdateCreditCardLimitCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public decimal NewCreditLimit { get; set; }
    }

    public class UpdateCreditCardLimitCommandHandler : IRequestHandler<UpdateCreditCardLimitCommand, Unit>
    {
        private readonly ICreditCardRepository _repository;

        public UpdateCreditCardLimitCommandHandler(ICreditCardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateCreditCardLimitCommand command, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(command.Id);
            if (entity == null) throw new Exception("CreditCard not found");

            if (command.NewCreditLimit < entity.CurrentDebt)
                throw new Exception("New limit cannot be less than current debt");

            entity.CreditLimit = command.NewCreditLimit;

            await _repository.UpdateAsync(command.Id, entity);
            return Unit.Value;
        }
    }
}
