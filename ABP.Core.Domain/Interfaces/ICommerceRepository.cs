using ABP.Core.Domain.Entities;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface ICommerceRepository : IGenericRepository<Commerce>
    {
        Task<Commerce?> GetByRncAsync(string rnc);
        Task<Commerce?> GetByUserIdAsync(string userId);
    }
}
