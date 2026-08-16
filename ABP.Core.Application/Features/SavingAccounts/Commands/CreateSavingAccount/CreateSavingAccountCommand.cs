
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.CreateSavingAccount
{
    /// <summary>
    /// Parameters required to create a new saving account
    /// </summary>
    public class CreateSavingAccountCommand : IRequest<int>
    {
        [SwaggerParameter(Description = "The generated account number")]
        public string AccountNumber { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The Client ID assigned to the account")]
        public string ClientId { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The initial balance of the account")]
        public decimal Balance { get; set; }

        [SwaggerParameter(Description = "The type of the saving account")]
        public SavingAccountType AccountType { get; set; }
    }

    public class CreateSavingAccountCommandHandler : IRequestHandler<CreateSavingAccountCommand, int>
    {
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly ILogger _logger;

        public CreateSavingAccountCommandHandler(ISavingAccountRepository savingAccountRepository, ILoggerFactory loggerFactory)
        {
            _savingAccountRepository = savingAccountRepository;
            _logger = loggerFactory.CreateLogger<CreateSavingAccountCommandHandler>();
        }

        public async Task<int> Handle(CreateSavingAccountCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating saving account for ClientId: {ClientId}, Type: {AccountType}", command.ClientId, command.AccountType);

            var entity = new SavingAccount
            {
                AccountNumber = command.AccountNumber,
                ClientId = command.ClientId,
                Balance = command.Balance,
                AccountType = command.AccountType,
                Status = SavingAccountStatus.Active
            };

            var result = await _savingAccountRepository.AddAsync(entity);

            _logger.LogInformation("Saving account creation result: {Result}", result != null ? "Success" : "Failure");
            
            if (result == null)
            {
                throw new Exception("Error creating saving account");
            }

            return result.Id;
        }
    }
}