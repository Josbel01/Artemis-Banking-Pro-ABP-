namespace ABP.Core.Application.Dtos.Transactions
{
    public class SaveCashAdvanceDto
    {
        public string OriginCreditCardNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
