using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ArtemisBankingAppContext context, Microsoft.Extensions.Logging.ILogger<GenericRepository<Transaction>> logger) : base(context, logger)
        {
        }

        public async Task<List<Transaction>> GetAllBySavingAccountIdAsync(int savingAccountId)
        {
            return await _context.Transactions
                .Where(t => t.SavingAccountId == savingAccountId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
