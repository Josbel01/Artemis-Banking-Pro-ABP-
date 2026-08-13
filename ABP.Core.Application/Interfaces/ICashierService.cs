using ABP.Core.Application.Dtos.Cashier;
using ABP.Core.Application.Dtos.Transactions;

namespace ABP.Core.Application.Interfaces
{
    public interface ICashierService
    {
        /// <summary>Deposita un monto en la cuenta indicada.</summary>
        Task<OperationResultDto> DepositAsync(CashierDepositDto dto);

        /// <summary>Retira un monto de la cuenta indicada.</summary>
        Task<OperationResultDto> WithdrawalAsync(CashierWithdrawalDto dto);

        /// <summary>Realiza un pago a una tarjeta de crédito.</summary>
        Task<OperationResultDto> CreditCardPaymentAsync(CashierCreditCardPaymentDto dto);

        /// <summary>Realiza un pago a un préstamo.</summary>
        Task<OperationResultDto> LoanPaymentAsync(CashierLoanPaymentDto dto);

        /// <summary>Transfiere fondos entre cuentas de terceros.</summary>
        Task<OperationResultDto> TransferBetweenAccountsAsync(CashierTransferDto dto);

        /// <summary>Retorna los indicadores diarios de operaciones del cajero.</summary>
        Task<DailyIndicatorsDto> GetDailyIndicatorsAsync(string cashierUserId);

        /// <summary>Retorna todas las transacciones del día realizadas por el cajero.</summary>
        Task<List<TransactionDto>> GetDailyTransactionsByCashierAsync(string cashierUserId);
    }
}
