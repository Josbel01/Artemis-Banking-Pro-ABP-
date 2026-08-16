
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.Transactions.Commands.CreateTransaction
{
    /// <summary>
    /// Parameters required to create a new transaction
    /// </summary>
    public class CreateTransactionCommand : IRequest<int>
    {
        [SwaggerParameter(Description = "The ID of the saving account")]
        public int SavingAccountId { get; set; }

        [SwaggerParameter(Description = "The amount of the transaction")]
        public decimal Amount { get; set; }

        [SwaggerParameter(Description = "The type of the transaction")]
        public TransactionType Type { get; set; }

        [SwaggerParameter(Description = "The beneficiary of the transaction, if applicable")]
        public string Beneficiary { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The origin of the transaction")]
        public string Origin { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The user ID of the responsible admin/cashier, if any")]
        public string? ResponsibleUserId { get; set; }
    }

    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, int>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger _logger;

        public CreateTransactionCommandHandler(ITransactionRepository transactionRepository, ILoggerFactory loggerFactory)
        {
            _transactionRepository = transactionRepository;
            _logger = loggerFactory.CreateLogger<CreateTransactionCommandHandler>();
        }

        public async Task<int> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating transaction for AccountId: {SavingAccountId}, Type: {Type}", command.SavingAccountId, command.Type);

            var entity = new Transaction
            {
                SavingAccountId = command.SavingAccountId,
                TransactionDate = DateTime.UtcNow,
                Amount = command.Amount,
                Type = command.Type,
                Beneficiary = command.Beneficiary,
                Origin = command.Origin,
                Status = TransactionStatus.Approved,
                ResponsibleUserId = command.ResponsibleUserId
            };

            var result = await _transactionRepository.AddAsync(entity);

            _logger.LogInformation("Transaction creation result: {Result}", result != null ? "Success" : "Failure");
            
            if (result == null)
            {
                throw new Exception("Error creating transaction");
            }

            return result.Id;
        }
    }
}