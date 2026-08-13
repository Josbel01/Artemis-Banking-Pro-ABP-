namespace ABP.Core.Application.Dtos.Cashier
{
    public class DailyIndicatorsDto
    {
        public int TotalDeposits { get; set; }
        public int TotalWithdrawals { get; set; }
        public int TotalCreditCardPayments { get; set; }
        public int TotalLoanPayments { get; set; }
        public int TotalTransfers { get; set; }
        public decimal TotalAmountOperated { get; set; }

        public int TotalOperations =>
            TotalDeposits + TotalWithdrawals + TotalCreditCardPayments + TotalLoanPayments + TotalTransfers;
    }
}
