using ABP.Core.Application.Dtos.Loans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface ILoanInstallmentService : IGenericService<LoanInstallmentDto>
    {
        Task<List<LoanInstallmentDto>> GetAllByLoanIdAsync(int loanId);
    }
}
