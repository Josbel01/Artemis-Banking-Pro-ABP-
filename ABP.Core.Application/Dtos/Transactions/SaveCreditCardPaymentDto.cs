namespace ABP.Core.Application.Dtos.Transactions
{
    public class SaveCreditCardPaymentDto
    {
        public string CreditCardNumber { get; set; } = string.Empty;
        public string OriginAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
