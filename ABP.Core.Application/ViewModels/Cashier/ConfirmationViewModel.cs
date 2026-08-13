namespace ABP.Core.Application.ViewModels.Cashier
{
    public class ConfirmationViewModel
    {
        public string OperationType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
        public DateTime OperationDate { get; set; } = DateTime.Now;
        public int TransactionId { get; set; }
    }
}
