using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, RegisterResponseDto>
    {
        private readonly IAccountServiceWebApi _accountService;
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;

        public CreateUserCommandHandler(
            IAccountServiceWebApi accountService,
            ISavingAccountService savingAccountService,
            ITransactionService transactionService)
        {
            _accountService = accountService;
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
        }

        public async Task<RegisterResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (request.UserDto.Role == "Commerce" || request.UserDto.Role == "Comercio")
            {
                throw new System.Exception("No se puede crear un usuario con rol Comercio desde este endpoint.");
            }

            var response = await _accountService.RegisterUser(request.UserDto, request.Origin, true);
            if (response.HasError) return response;

            if (request.UserDto.Role == "Client" || request.UserDto.Role == "Cliente")
            {
                var rnd = new System.Random();
                string accountNumber = rnd.Next(100000000, 999999999).ToString();
                
                var newAccount = new ABP.Core.Application.Dtos.SavingAccounts.SavingAccountDto
                {
                    Id = 0,
                    ClientId = response.Id,
                    AccountNumber = accountNumber,
                    Balance = request.UserDto.InitialAmount ?? 0,
                    AccountType = ABP.Core.Domain.Common.Enums.SavingAccountType.Main,
                    Status = ABP.Core.Domain.Common.Enums.SavingAccountStatus.Active
                };
                
                var createdAccount = await _savingAccountService.AddAsync(newAccount);

                if (request.UserDto.InitialAmount > 0)
                {
                    await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                    {
                        SavingAccountId = createdAccount.Id,
                        Amount = request.UserDto.InitialAmount.Value,
                        Type = ABP.Core.Domain.Common.Enums.TransactionType.Credit,
                        TransactionDate = System.DateTime.Now,
                        Origin = "Apertura de Cuenta",
                        Beneficiary = createdAccount.AccountNumber,
                        Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
                    });
                }
            }

            return response;
        }
    }
}
