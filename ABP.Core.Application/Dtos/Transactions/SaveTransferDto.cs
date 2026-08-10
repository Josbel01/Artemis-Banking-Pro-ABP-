namespace ABP.Core.Application.Dtos.Transactions
{
    public class SaveTransferDto
    {
        public string OriginAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
