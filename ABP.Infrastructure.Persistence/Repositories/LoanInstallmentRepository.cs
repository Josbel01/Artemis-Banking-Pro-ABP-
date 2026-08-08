using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class LoanInstallmentRepository : GenericRepository<LoanInstallment>, ILoanInstallmentRepository
    {
        public LoanInstallmentRepository(ArtemisBankingAppContext context) : base(context)
        {
        }

        public async Task<List<LoanInstallment>> GetAllByLoanIdAsync(int loanId)
        {
            return await _context.LoanInstallments
                .Where(li => li.LoanId == loanId)
                .OrderBy(li => li.InstallmentNumber)
                .ToListAsync();
        }
    }
}
