using ABP.Core.Application.Dtos.SavingAccounts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface ISavingAccountService : IGenericService<SavingAccountDto>
    {
        Task<List<SavingAccountDto>> GetAllByClientIdAsync(string clientId);
        Task<SavingAccountDto?> GetByAccountNumberAsync(string accountNumber);
    }
}
