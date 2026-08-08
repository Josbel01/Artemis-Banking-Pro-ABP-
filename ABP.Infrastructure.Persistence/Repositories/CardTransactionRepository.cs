using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class CardTransactionRepository : GenericRepository<CardTransaction>, ICardTransactionRepository
    {
        public CardTransactionRepository(ArtemisBankingAppContext context) : base(context)
        {
        }

        public async Task<List<CardTransaction>> GetAllByCreditCardIdAsync(int creditCardId)
        {
            return await _context.CardTransactions
                .Where(ct => ct.CreditCardId == creditCardId)
                .OrderByDescending(ct => ct.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<CardTransaction>> GetAllByCommerceIdAsync(int commerceId)
        {
            return await _context.CardTransactions
                .Where(ct => ct.CommerceId == commerceId)
                .OrderByDescending(ct => ct.TransactionDate)
                .ToListAsync();
        }
    }
}
