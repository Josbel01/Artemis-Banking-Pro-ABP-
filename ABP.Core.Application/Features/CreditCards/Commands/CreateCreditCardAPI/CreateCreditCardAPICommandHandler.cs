using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.CreditCards;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.CreditCards.Commands.CreateCreditCardAPI
{
    public class CreateCreditCardAPICommandHandler : IRequestHandler<CreateCreditCardAPICommand, object>
    {
        private readonly ICreditCardService _creditCardService;

        public CreateCreditCardAPICommandHandler(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
        }

        public async Task<object> Handle(CreateCreditCardAPICommand request, CancellationToken cancellationToken)
        {
            var card = new CreditCardDto
            {
                Id = 0,
                ClientId = request.ClientId,
                CreditLimit = request.CreditLimit,
                CurrentDebt = 0,
                Status = CreditCardStatus.Active,
                CardNumber = new Random().NextInt64(1000000000000000, 9999999999999999).ToString(),
                Cvc = new Random().Next(100, 999).ToString(),
                ExpirationDate = DateTime.Now.AddYears(3).ToString("MM/yy")
            };

            var created = await _creditCardService.AddAsync(card);

            return new
            {
                id = created.Id.ToString(),
                maskedCardNumber = $"************{created.CardNumber.Substring(created.CardNumber.Length - 4)}",
                lastFourDigits = created.CardNumber.Substring(created.CardNumber.Length - 4),
                clientId = created.ClientId,
                clientFullName = "",
                creditLimit = created.CreditLimit,
                availableCredit = created.CreditLimit,
                currentDebt = created.CurrentDebt,
                expirationDate = created.ExpirationDate,
                status = "Activa",
                createdAt = DateTime.Now
            };
        }
    }
}

