using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class SavingAccountRepository : GenericRepository<SavingAccount>, ISavingAccountRepository
    {
        public SavingAccountRepository(ArtemisBankingAppContext context, Microsoft.Extensions.Logging.ILogger<GenericRepository<SavingAccount>> logger) : base(context, logger)
        {
        }

        public async Task<SavingAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.SavingAccounts
                .Include(s => s.Transactions.OrderByDescending(t => t.TransactionDate))
                .FirstOrDefaultAsync(s => s.AccountNumber == accountNumber);
        }

        public async Task<List<SavingAccount>> GetAllByClientIdAsync(string clientId)
        {
            return await _context.SavingAccounts
                .Where(s => s.ClientId == clientId && s.Status == SavingAccountStatus.Active)
                .OrderBy(s => s.AccountType)
                .ThenByDescending(s => s.Balance)
                .ToListAsync();
        }

        public async Task<SavingAccount?> GetPrincipalAccountByClientIdAsync(string clientId)
        {
            return await _context.SavingAccounts
                .FirstOrDefaultAsync(s => s.ClientId == clientId && s.AccountType == SavingAccountType.Main && s.Status == SavingAccountStatus.Active);
        }

        public async Task<bool> ExistsAccountNumberAsync(string accountNumber)
        {
            var existsInAccounts = await _context.SavingAccounts.AnyAsync(s => s.AccountNumber == accountNumber);
            if (existsInAccounts) return true;

            var existsInLoans = await _context.Loans.AnyAsync(l => l.LoanNumber == accountNumber);
            return existsInLoans;
        }
    }
}
