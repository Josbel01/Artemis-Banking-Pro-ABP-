using ABP.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using ABP.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Persistence.Repositories
{
    public class GenericRepository<Entity> : IGenericRepository<Entity> where Entity : class
    {
        protected readonly ArtemisBankingAppContext _context;
        protected readonly Microsoft.Extensions.Logging.ILogger<GenericRepository<Entity>> _logger;

        public GenericRepository(ArtemisBankingAppContext context, Microsoft.Extensions.Logging.ILogger<GenericRepository<Entity>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public virtual async Task<Entity?> AddAsync(Entity entity)
        {
            _logger.LogInformation("Adding new entity of type {EntityType}", typeof(Entity).Name);
            await _context.Set<Entity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Entity of type {EntityType} added successfully", typeof(Entity).Name);
            return entity;
        }

        public virtual async Task<List<Entity>?> AddRangeAsync(List<Entity> entities)
        {
            await _context.Set<Entity>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
            return entities;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<Entity>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<Entity>().Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entity of type {EntityType} with ID {Id} deleted successfully", typeof(Entity).Name, id);
            }
        }

        public virtual async Task<List<Entity>> GetAllListAsync()
        {
            return await _context.Set<Entity>().ToListAsync();
        }

        public virtual async Task<List<Entity>> GetAllListWithInclude(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();
            foreach (var property in properties)
            {
                query = query.Include(property);
            }
            return await query.ToListAsync();
        }

        public virtual IQueryable<Entity> GetAllQuery()
        {
            return _context.Set<Entity>().AsQueryable();
        }

        public virtual IQueryable<Entity> GetAllQueryWithInclude(List<string> properties)
        {
            var query = _context.Set<Entity>().AsQueryable();
            foreach (var property in properties)
            {
                query = query.Include(property);
            }
            return query;
        }

        public virtual async Task<Entity?> GetByIdAsync(int id)
        {
            return await _context.Set<Entity>().FindAsync(id);
        }

        public virtual async Task<Entity?> UpdateAsync(int id, Entity entity)
        {
            _logger.LogInformation("Updating entity of type {EntityType} with ID: {Id}", typeof(Entity).Name, id);
            var entry = await _context.Set<Entity>().FindAsync(id);
            if (entry != null)
            {
                _context.Entry(entry).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return entry;
            }
            _logger.LogWarning("Entity with ID {Id} not found for update", id);
            return null;
        }
    }
}
