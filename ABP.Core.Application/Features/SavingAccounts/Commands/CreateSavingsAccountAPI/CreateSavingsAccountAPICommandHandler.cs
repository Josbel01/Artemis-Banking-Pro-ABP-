using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.SavingAccounts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using ABP.Core.Domain.Common.Enums;

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
            var account = new SavingAccountDto
            {
                Id = 0,
                ClientId = request.ClientId,
                Balance = request.InitialBalance,
                AccountType = SavingAccountType.Secondary,
                Status = SavingAccountStatus.Active,
                AccountNumber = new Random().Next(100000000, 999999999).ToString(),
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
    }
}
