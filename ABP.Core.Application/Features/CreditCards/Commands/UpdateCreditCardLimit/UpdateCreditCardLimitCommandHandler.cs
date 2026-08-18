using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CreditCards.Commands.UpdateCreditCardLimit
{
    public class UpdateCreditCardLimitCommandHandler : IRequestHandler<UpdateCreditCardLimitCommand, bool>
    {
        private readonly ICreditCardService _creditCardService;

        public UpdateCreditCardLimitCommandHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        public async Task<bool> Handle(UpdateCreditCardLimitCommand request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return false;

            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return false;

            // Simplified: we would normally validate against current debt
            card.CreditLimit = request.CreditLimit;
            await _creditCardService.UpdateAsync(card, id);

            return true;
        }
    }
}

