namespace ABP.Core.Application.Dtos.Transactions
{
    public class SaveLoanPaymentDto
    {
        public string LoanNumber { get; set; } = string.Empty;
        public string OriginAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
