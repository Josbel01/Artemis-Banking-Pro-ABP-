namespace ABP.Core.Application.Dtos.Cashier
{
    public class CashierTransferDto
    {
        public string OriginAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ResponsibleUserId { get; set; }
    }
}
