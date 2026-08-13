using ABP.Core.Application.Dtos.Cashier;

namespace ABP.Core.Application.ViewModels.Cashier
{
    /// <summary>
    /// ViewModel del dashboard del cajero con indicadores diarios.
    /// </summary>
    public class CashierHomeViewModel
    {
        public int TotalDeposits { get; set; }
        public int TotalWithdrawals { get; set; }
        public int TotalCreditCardPayments { get; set; }
        public int TotalLoanPayments { get; set; }
        public int TotalTransfers { get; set; }
        public decimal TotalAmountOperated { get; set; }

        public int TotalOperations =>
            TotalDeposits + TotalWithdrawals + TotalCreditCardPayments + TotalLoanPayments + TotalTransfers;

        public static CashierHomeViewModel FromDto(DailyIndicatorsDto dto) => new()
        {
            TotalDeposits = dto.TotalDeposits,
            TotalWithdrawals = dto.TotalWithdrawals,
            TotalCreditCardPayments = dto.TotalCreditCardPayments,
            TotalLoanPayments = dto.TotalLoanPayments,
            TotalTransfers = dto.TotalTransfers,
            TotalAmountOperated = dto.TotalAmountOperated
        };
    }
}
