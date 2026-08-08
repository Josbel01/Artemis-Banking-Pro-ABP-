using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class BeneficiaryRepository : GenericRepository<Beneficiary>, IBeneficiaryRepository
    {
        public BeneficiaryRepository(ArtemisBankingAppContext context) : base(context)
        {
        }

        public async Task<List<Beneficiary>> GetAllByClientIdAsync(string clientId)
        {
            return await _context.Beneficiaries
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Beneficiary?> GetByAccountAndClientIdAsync(string accountNumber, string clientId)
        {
            return await _context.Beneficiaries
                .FirstOrDefaultAsync(b => b.BeneficiaryAccountNumber == accountNumber && b.ClientId == clientId);
        }
    }
}
