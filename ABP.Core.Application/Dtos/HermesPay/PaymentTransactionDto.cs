namespace ABP.Core.Application.Dtos.HermesPay
{
    public class PaymentTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string CardLastFourDigits { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentTransactionResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CommerceId { get; set; }
        public string CommerceName { get; set; } = string.Empty;
        public List<PaymentTransactionDto> Data { get; set; } = new List<PaymentTransactionDto>();
    }
}
