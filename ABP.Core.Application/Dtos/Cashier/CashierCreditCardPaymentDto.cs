namespace ABP.Core.Application.Dtos.Cashier
{
    public class CashierCreditCardPaymentDto
    {
        public string CardNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ResponsibleUserId { get; set; }
    }
}
