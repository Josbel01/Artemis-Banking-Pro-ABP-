
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CardTransactions.Commands.CreateCardTransaction
{
    /// <summary>
    /// Parameters required to create a new card transaction
    /// </summary>
    public class CreateCardTransactionCommand : IRequest<int>
    {
        [SwaggerParameter(Description = "The ID of the credit card")]
        public int CreditCardId { get; set; }

        [SwaggerParameter(Description = "The optional ID of the commerce if applicable")]
        public int? CommerceId { get; set; }

        [SwaggerParameter(Description = "The amount of the transaction")]
        public decimal Amount { get; set; }

        [SwaggerParameter(Description = "The name of the commerce if not registered")]
        public string CommerceName { get; set; } = string.Empty;
    }

    public class CreateCardTransactionCommandHandler : IRequestHandler<CreateCardTransactionCommand, int>
    {
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly ILogger _logger;

        public CreateCardTransactionCommandHandler(ICardTransactionRepository cardTransactionRepository, ILoggerFactory loggerFactory)
        {
            _cardTransactionRepository = cardTransactionRepository;
            _logger = loggerFactory.CreateLogger<CreateCardTransactionCommandHandler>();
        }

        public async Task<int> Handle(CreateCardTransactionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating card transaction for CreditCardId: {CreditCardId}, Amount: {Amount}", command.CreditCardId, command.Amount);

            var entity = new CardTransaction
            {
                CreditCardId = command.CreditCardId,
                CommerceId = command.CommerceId,
                Amount = command.Amount,
                CommerceName = command.CommerceName ?? string.Empty,
                TransactionDate = DateTime.UtcNow,
                Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
            };

            var result = await _cardTransactionRepository.AddAsync(entity);

            _logger.LogInformation("Card transaction creation result: {Result}", result != null ? "Success" : "Failure");
            
            if (result == null)
            {
                throw new Exception("Error creating card transaction");
            }

            return result.Id;
        }
    }
}