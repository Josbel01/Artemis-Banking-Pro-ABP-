using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.SavingAccounts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Application.Exceptions;
using System.Linq;
using System.Security.Cryptography;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.CreateSavingsAccountAPI
{
    public class CreateSavingsAccountAPICommandHandler : IRequestHandler<CreateSavingsAccountAPICommand, object>
    {
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;

        public CreateSavingsAccountAPICommandHandler(ISavingAccountService savingAccountService, ITransactionService transactionService)
        {
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
        }

        public async Task<object> Handle(CreateSavingsAccountAPICommand request, CancellationToken cancellationToken)
        {
            if (request.InitialBalance < 0)
                throw new ApiException("El balance inicial no puede ser negativo.");

            var clientAccounts = await _savingAccountService.GetAllByClientIdAsync(request.ClientId);
            if (!clientAccounts.Any(account => account.AccountType == SavingAccountType.Main && account.Status == SavingAccountStatus.Active))
                throw new ApiException("El cliente debe tener una cuenta principal activa.");

            var allAccounts = await _savingAccountService.GetAllAsync();
            var existingNumbers = allAccounts.Select(account => account.AccountNumber).ToHashSet();
            var accountNumber = GenerateUniqueAccountNumber(existingNumbers);
            var account = new SavingAccountDto
            {
                Id = 0,
                ClientId = request.ClientId,
                Balance = request.InitialBalance,
                AccountType = SavingAccountType.Secondary,
                Status = SavingAccountStatus.Active,
                AccountNumber = accountNumber,
            };

            var created = await _savingAccountService.AddAsync(account);

            if (request.InitialBalance > 0 && created != null)
            {
                await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                {
                    Id = 0,
                    SavingAccountId = created.Id,
                    Amount = request.InitialBalance,
                    Type = TransactionType.Credit,
                    TransactionDate = DateTime.Now,
                    Origin = "Apertura",
                    Beneficiary = created.AccountNumber
                });
            }

            return new
            {
                id = created.Id.ToString(),
                accountNumber = created.AccountNumber,
                clientId = created.ClientId,
                clientFullName = "", 
                balance = created.Balance,
                type = "Secundaria",
                status = "Activa",
                createdAt = DateTime.Now
            };
        }

        private static string GenerateUniqueAccountNumber(ISet<string> existingNumbers)
        {
            for (var attempt = 0; attempt < 20; attempt++)
            {
                var number = RandomNumberGenerator.GetInt32(100_000_000, 1_000_000_000).ToString();
                if (!existingNumbers.Contains(number)) return number;
            }
            throw new ApiException("No fue posible generar un número de cuenta único.");
        }
    }
}
