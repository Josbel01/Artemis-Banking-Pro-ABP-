using ABP.Core.Application.Dtos.Beneficiaries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface IBeneficiaryService : IGenericService<BeneficiaryDto>
    {
        Task<List<BeneficiaryDto>> GetAllByClientIdAsync(string clientId);
    }
}
