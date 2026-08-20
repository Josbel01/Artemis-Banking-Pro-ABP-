namespace ABP.Core.Application.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Indicadores de Productos Financieros
        public int ActiveLoans { get; set; }
        public int ActiveCreditCards { get; set; }
        public int ActiveSavingAccounts { get; set; }
        public int TotalFinancialProducts => ActiveLoans + ActiveCreditCards + ActiveSavingAccounts;

        // Indicadores de Usuarios
        public int ActiveClients { get; set; }
        public int InactiveClients { get; set; }

        // Indicadores de Transacciones
        public int TotalHistoricalTransactions { get; set; }
        public int TodayTransactions { get; set; }

        // Indicadores de Pagos
        public int TotalHistoricalPayments { get; set; }
        public int TodayPayments { get; set; }

        // Deuda Promedio
        public decimal AverageDebtPerClient { get; set; }
    }
}
