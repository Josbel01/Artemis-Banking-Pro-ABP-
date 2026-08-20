namespace ABP.Core.Application.Dtos.Cashier
{
    public class OperationResultDto
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        // Datos para la pantalla de confirmación
        public string OperationType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string DestinationHolderName { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
        public DateTime OperationDate { get; set; } = DateTime.Now;
        public int TransactionId { get; set; }
    }
}
