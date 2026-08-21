using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {
        public LoanRepository(ArtemisBankingAppContext context, Microsoft.Extensions.Logging.ILogger<GenericRepository<Loan>> logger) : base(context, logger)
        {
        }

        public async Task<Loan?> GetByLoanNumberAsync(string loanNumber)
        {
            var cleaned = loanNumber.Replace(" ", "").Trim();
            return await _context.Loans
                .Include(l => l.LoanInstallments.OrderBy(li => li.InstallmentNumber))
                .FirstOrDefaultAsync(l => l.LoanNumber.Replace(" ", "") == cleaned);
        }

        public async Task<List<Loan>> GetAllByClientIdAsync(string clientId)
        {
            return await _context.Loans
                .Include(l => l.LoanInstallments.OrderBy(li => li.InstallmentNumber))
                .Where(l => l.ClientId == clientId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public override async Task<List<Loan>> GetAllListAsync()
        {
            return await _context.Loans
                .Include(l => l.LoanInstallments)
                .OrderByDescending(l => l.Id)
                .ToListAsync();
        }

        public async Task<bool> ExistsLoanNumberAsync(string loanNumber)
        {
            var existsInLoans = await _context.Loans.AnyAsync(l => l.LoanNumber == loanNumber);
            if (existsInLoans) return true;

            var existsInAccounts = await _context.SavingAccounts.AnyAsync(s => s.AccountNumber == loanNumber);
            return existsInAccounts;
        }
    }
}
