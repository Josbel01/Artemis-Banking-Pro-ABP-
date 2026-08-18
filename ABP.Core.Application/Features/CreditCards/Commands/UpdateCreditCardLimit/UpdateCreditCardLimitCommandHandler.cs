using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Exceptions;

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

            if (request.CreditLimit < card.CurrentDebt)
                throw new ApiException("El nuevo límite no puede ser menor que la deuda actual.");

            card.CreditLimit = request.CreditLimit;
            await _creditCardService.UpdateAsync(card, id);

            return true;
        }
    }
}

