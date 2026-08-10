using ABP.Core.Application.Dtos.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface ITransactionService : IGenericService<TransactionDto>
    {
        Task<List<TransactionDto>> GetTransactionsByAccountIdAsync(int accountId);
        Task<bool> TransferAsync(SaveTransferDto dto);
        Task<bool> CashAdvanceAsync(SaveCashAdvanceDto dto);
    }
}
