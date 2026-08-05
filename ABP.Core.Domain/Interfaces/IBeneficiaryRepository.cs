using ABP.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface IBeneficiaryRepository : IGenericRepository<Beneficiary>
    {
        Task<List<Beneficiary>> GetAllByClientIdAsync(string clientId);
        Task<Beneficiary?> GetByAccountAndClientIdAsync(string accountNumber, string clientId);
    }
}
