
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CreditCards.Commands.CreateCreditCard
{
    /// <summary>
    /// Parameters required to create a new credit card
    /// </summary>
    public class CreateCreditCardCommand : IRequest<int>
    {
        [SwaggerParameter(Description = "The generated or assigned card number")]
        public string CardNumber { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The Client ID assigned to the card")]
        public string ClientId { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The credit limit for the card")]
        public decimal CreditLimit { get; set; }

        [SwaggerParameter(Description = "The expiration date in MM/AA format")]
        public string ExpirationDate { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The CVC security code")]
        public string Cvc { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The user ID of the admin who assigned the card")]
        public string AssignedByUserId { get; set; } = string.Empty;
    }

    public class CreateCreditCardCommandHandler : IRequestHandler<CreateCreditCardCommand, int>
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ILogger _logger;

        public CreateCreditCardCommandHandler(ICreditCardRepository creditCardRepository, ILoggerFactory loggerFactory)
        {
            _creditCardRepository = creditCardRepository;
            _logger = loggerFactory.CreateLogger<CreateCreditCardCommandHandler>();
        }

        public async Task<int> Handle(CreateCreditCardCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating credit card for ClientId: {ClientId}, Limit: {CreditLimit}", command.ClientId, command.CreditLimit);

            var entity = new CreditCard
            {
                CardNumber = command.CardNumber,
                ClientId = command.ClientId,
                CreditLimit = command.CreditLimit,
                CurrentDebt = 0m,
                ExpirationDate = command.ExpirationDate,
                Cvc = command.Cvc,
                AssignedByUserId = command.AssignedByUserId,
                Status = ABP.Core.Domain.Common.Enums.CreditCardStatus.Active
            };

            var result = await _creditCardRepository.AddAsync(entity);

            _logger.LogInformation("Credit card creation result: {Result}", result != null ? "Success" : "Failure");
            
            if (result == null)
            {
                throw new Exception("Error creating credit card");
            }

            return result.Id;
        }
    }
}