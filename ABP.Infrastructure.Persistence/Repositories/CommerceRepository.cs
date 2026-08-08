using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class CommerceRepository : GenericRepository<Commerce>, ICommerceRepository
    {
        public CommerceRepository(ArtemisBankingAppContext context) : base(context)
        {
        }

        public async Task<Commerce?> GetByRncAsync(string rnc)
        {
            return await _context.Commerces
                .FirstOrDefaultAsync(c => c.Rnc == rnc);
        }

        public async Task<Commerce?> GetByUserIdAsync(string userId)
        {
            return await _context.Commerces
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Commerce?> GetByEmailAsync(string email)
        {
            return await _context.Commerces
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> ExistsRncAsync(string rnc)
        {
            return await _context.Commerces.AnyAsync(c => c.Rnc == rnc);
        }

        public async Task<bool> ExistsEmailAsync(string email)
        {
            return await _context.Commerces.AnyAsync(c => c.Email == email);
        }
    }
}
