using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ABP.Core.Domain.Common.Enums;
using System.Transactions;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.DeleteSavingsAccountAPI
{
    public class DeleteSavingsAccountAPICommandHandler : IRequestHandler<DeleteSavingsAccountAPICommand, bool>
    {
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;

        public DeleteSavingsAccountAPICommandHandler(ISavingAccountService savingAccountService, ITransactionService transactionService)
        {
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
        }

        public async Task<bool> Handle(DeleteSavingsAccountAPICommand request, CancellationToken cancellationToken)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var accounts = await _savingAccountService.GetAllAsync();
            var account = accounts.FirstOrDefault(a => a.AccountNumber == request.AccountNumber);
            
            if (account == null) return false;
            if (account.AccountType == SavingAccountType.Main) throw new System.Exception("Las cuentas principales no pueden ser canceladas.");

            var dbAccount = await _savingAccountService.GetByIdAsync(account.Id);
            
            if (dbAccount.Balance > 0)
            {
                var mainAccount = accounts.FirstOrDefault(a => a.ClientId == account.ClientId && a.AccountType == SavingAccountType.Main);
                if (mainAccount != null)
                {
                    var dbMainAccount = await _savingAccountService.GetByIdAsync(mainAccount.Id);
                    dbMainAccount.Balance += dbAccount.Balance;
                    await _savingAccountService.UpdateAsync(dbMainAccount, mainAccount.Id);

                    await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                    {
                        Id = 0,
                        SavingAccountId = dbMainAccount.Id,
                        Amount = dbAccount.Balance,
                        Type = TransactionType.Credit,
                        TransactionDate = System.DateTime.Now,
                        Origin = dbAccount.AccountNumber,
                        Beneficiary = dbMainAccount.AccountNumber
                    });
                }
                
                await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                {
                    Id = 0,
                    SavingAccountId = dbAccount.Id,
                    Amount = dbAccount.Balance,
                    Type = TransactionType.Debit,
                    TransactionDate = System.DateTime.Now,
                    Origin = dbAccount.AccountNumber,
                    Beneficiary = mainAccount?.AccountNumber ?? ""
                });

                dbAccount.Balance = 0;
            }

            dbAccount.Status = SavingAccountStatus.Cancelled;
            await _savingAccountService.UpdateAsync(dbAccount, dbAccount.Id);

            scope.Complete();
            return true;
        }
    }
}
