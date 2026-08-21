namespace ABP.Core.Application.ViewModels.Cashier
{
    public class PreConfirmOperationViewModel
    {
        public string OperationType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string DestinationHolderName { get; set; } = string.Empty;
        public string ResponsibleUserId { get; set; } = string.Empty;
    }
}
