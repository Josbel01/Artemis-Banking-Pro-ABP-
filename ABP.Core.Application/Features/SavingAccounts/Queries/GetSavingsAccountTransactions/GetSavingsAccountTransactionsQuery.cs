using MediatR;

namespace ABP.Core.Application.Features.SavingAccounts.Queries.GetSavingsAccountTransactions
{
    public class GetSavingsAccountTransactionsQuery : IRequest<object>
    {
        public string AccountNumber { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
