namespace ABP.Core.Application.Dtos.Cashier
{
    public class CashierDepositDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ResponsibleUserId { get; set; }
    }
}
