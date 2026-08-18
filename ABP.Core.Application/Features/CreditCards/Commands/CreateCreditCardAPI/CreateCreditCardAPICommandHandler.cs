using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.CreditCards;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Application.Exceptions;
using System.Security.Cryptography;
using System.Text;

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
            if (request.CreditLimit <= 0)
                throw new ApiException("El límite de crédito debe ser mayor que cero.");

            var existingCards = await _creditCardService.GetAllAsync();
            var cardNumber = GenerateUniqueCardNumber(existingCards.Select(card => card.CardNumber));
            var cvc = RandomNumberGenerator.GetInt32(100, 1000).ToString();

            var card = new CreditCardDto
            {
                Id = 0,
                ClientId = request.ClientId,
                CreditLimit = request.CreditLimit,
                CurrentDebt = 0,
                Status = CreditCardStatus.Active,
                CardNumber = cardNumber,
                Cvc = ComputeSha256Hash(cvc),
                ExpirationDate = DateTime.UtcNow.AddYears(3).ToString("MM/yy")
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

        private static string GenerateUniqueCardNumber(IEnumerable<string> existingNumbers)
        {
            var existing = existingNumbers.ToHashSet();
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var number = string.Concat(Enumerable.Range(0, 16)
                    .Select(_ => RandomNumberGenerator.GetInt32(0, 10).ToString()));
                if (!existing.Contains(number)) return number;
            }
            throw new ApiException("No fue posible generar un número de tarjeta único.");
        }

        private static string ComputeSha256Hash(string value) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }
}

