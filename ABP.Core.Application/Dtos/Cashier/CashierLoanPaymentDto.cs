namespace ABP.Core.Application.Dtos.Cashier
{
    public class CashierLoanPaymentDto
    {
        public string LoanNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OriginAccountNumber { get; set; } = string.Empty;
        public string? ResponsibleUserId { get; set; }
    }
}
