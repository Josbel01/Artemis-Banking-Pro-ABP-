using ABP.Core.Domain.Entities;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface ICommerceRepository : IGenericRepository<Commerce>
    {
        Task<Commerce?> GetByRncAsync(string rnc);
        Task<Commerce?> GetByUserIdAsync(string userId);
        Task<Commerce?> GetByEmailAsync(string email);
        Task<bool> ExistsRncAsync(string rnc);
        Task<bool> ExistsEmailAsync(string email);
    }
}
