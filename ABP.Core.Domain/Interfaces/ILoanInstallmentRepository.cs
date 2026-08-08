using ABP.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface ILoanInstallmentRepository : IGenericRepository<LoanInstallment>
    {
        Task<List<LoanInstallment>> GetAllByLoanIdAsync(int loanId);
    }
}
