using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CreditCards.Commands.CancelCreditCard
{
    public class CancelCreditCardCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }

    public class CancelCreditCardCommandHandler : IRequestHandler<CancelCreditCardCommand, Unit>
    {
        private readonly ICreditCardRepository _repository;

        public CancelCreditCardCommandHandler(ICreditCardRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(CancelCreditCardCommand command, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(command.Id);
            if (entity == null) throw new Exception("CreditCard not found");

            if (entity.CurrentDebt > 0)
                throw new Exception("Cannot cancel credit card with pending debt");

            entity.Status = CreditCardStatus.Cancelled;

            await _repository.UpdateAsync(command.Id, entity);
            return Unit.Value;
        }
    }
}
