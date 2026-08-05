using ABP.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<Loan?> GetByLoanNumberAsync(string loanNumber);
        Task<List<Loan>> GetAllByClientIdAsync(string clientId);
    }
}
