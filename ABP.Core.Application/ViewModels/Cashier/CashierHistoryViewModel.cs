using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.ViewModels.Cashier
{
    /// <summary>
    /// ViewModel para el historial diario de operaciones del cajero.
    /// </summary>
    public class CashierHistoryViewModel
    {
        public List<TransactionItemViewModel> Transactions { get; set; } = new();
        public int TotalDeposits => Transactions.Count(t => t.Type == TransactionType.Deposit);
        public int TotalWithdrawals => Transactions.Count(t => t.Type == TransactionType.Withdrawal);
        public int TotalTransfers => Transactions.Count(t => t.Type == TransactionType.Transfer);
        public int TotalLoanPayments => Transactions.Count(t => t.Type == TransactionType.LoanPayment);
        public decimal TotalAmountOperated => Transactions.Sum(t => t.Amount);

        public static CashierHistoryViewModel FromDtoList(List<TransactionDto> dtos) => new()
        {
            Transactions = dtos.Select(t => new TransactionItemViewModel
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                Origin = t.Origin,
                Beneficiary = t.Beneficiary,
                TransactionDate = t.TransactionDate,
                Status = t.Status
            }).ToList()
        };
    }

    public class TransactionItemViewModel
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Beneficiary { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public TransactionStatus Status { get; set; }
    }
}
