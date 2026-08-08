using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class CreditCardRepository : GenericRepository<CreditCard>, ICreditCardRepository
    {
        public CreditCardRepository(ArtemisBankingAppContext context) : base(context)
        {
        }

        public async Task<CreditCard?> GetByCardNumberAsync(string cardNumber)
        {
            return await _context.CreditCards
                .Include(c => c.CardTransactions.OrderByDescending(ct => ct.TransactionDate))
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
        }

        public async Task<List<CreditCard>> GetAllByClientIdAsync(string clientId)
        {
            return await _context.CreditCards
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsCardNumberAsync(string cardNumber)
        {
            return await _context.CreditCards.AnyAsync(c => c.CardNumber == cardNumber);
        }
    }
}
