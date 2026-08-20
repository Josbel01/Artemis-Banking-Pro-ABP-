using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.SavingAccounts.Queries.GetSavingsAccountTransactions
{
    public class GetSavingsAccountTransactionsQueryHandler : IRequestHandler<GetSavingsAccountTransactionsQuery, object?>
    {
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;

        public GetSavingsAccountTransactionsQueryHandler(ISavingAccountService savingAccountService, ITransactionService transactionService)
        {
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
        }

        public async Task<object?> Handle(GetSavingsAccountTransactionsQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _savingAccountService.GetAllAsync();
            var account = accounts.FirstOrDefault(a => a.AccountNumber == request.AccountNumber);
            
            if (account == null) return null;

            var transactions = await _transactionService.GetAllAsync();
            var accountTransactions = transactions.Where(t => t.SavingAccountId == account.Id || t.Beneficiary == request.AccountNumber).OrderByDescending(t => t.TransactionDate).ToList();

            var paged = accountTransactions.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new
            {
                accountNumber = account.AccountNumber,
                clientFullName = "",
                balance = account.Balance,
                type = account.AccountType == SavingAccountType.Main ? "Principal" : "Secundaria",
                status = account.Status == SavingAccountStatus.Active ? "Activa" : "Cancelada",
                transactions = new {
                    page = request.Page,
                    pageSize = request.PageSize,
                    totalRecords = accountTransactions.Count,
                    totalPages = accountTransactions.Count == 0 ? 1 : (int)Math.Ceiling(accountTransactions.Count / (double)request.PageSize),
                    data = paged.Select(t => new {
                        id = t.Id.ToString(),
                        date = t.TransactionDate,
                        amount = t.Amount,
                        transactionType = t.Type switch
                        {
                            TransactionType.Debit => "DÉBITO",
                            TransactionType.Credit => "CRÉDITO",
                            TransactionType.Deposit => "DEPÓSITO",
                            TransactionType.Withdrawal => "RETIRO",
                            TransactionType.CreditCardPayment => "PAGO TARJETA DE CRÉDITO",
                            TransactionType.LoanPayment => "PAGO PRÉSTAMO",
                            TransactionType.Transfer => "TRANSFERENCIA",
                            TransactionType.CashAdvance => "AVANCE DE EFECTIVO",
                            _ => t.Type.ToString()
                        },
                        origin = t.Origin ?? "Desconocido",
                        beneficiary = t.Beneficiary,
                        status = t.Status == TransactionStatus.Approved ? "APROBADA" : "RECHAZADO"
                    })
                }
            };
        }
    }
}
