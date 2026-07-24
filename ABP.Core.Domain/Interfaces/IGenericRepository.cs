using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface IGenericRepository<Entity> where Entity : class
    {
        Task<Entity?> GetByIdAsync(int id);
        Task<List<Entity>> GetAllListAsync();
        Task<Entity?> AddAsync(Entity entity);
        IQueryable<Entity> GetAllQuery();
        Task<Entity?> UpdateAsync(int id, Entity entity);
        Task DeleteAsync(int id);
        Task<List<Entity>?> AddRangeAsync(List<Entity> entities);
        Task<List<Entity>> GetAllListWithInclude(List<string> properties);
        IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);


    }
}
