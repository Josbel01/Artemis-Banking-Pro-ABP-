using ABP.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface ISavingAccountRepository : IGenericRepository<SavingAccount>
    {
        Task<SavingAccount?> GetByAccountNumberAsync(string accountNumber);
        Task<List<SavingAccount>> GetAllByClientIdAsync(string clientId);
    }
}
