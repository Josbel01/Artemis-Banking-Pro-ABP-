using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.CreditCards.Commands.DeleteCreditCardAPI
{
    public class DeleteCreditCardAPICommandHandler : IRequestHandler<DeleteCreditCardAPICommand, bool>
    {
        private readonly ICreditCardService _creditCardService;

        public DeleteCreditCardAPICommandHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        public async Task<bool> Handle(DeleteCreditCardAPICommand request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return false;

            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return false;

            card.Status = CreditCardStatus.Cancelled;
            await _creditCardService.UpdateAsync(card, id);

            return true;
        }
    }
}

