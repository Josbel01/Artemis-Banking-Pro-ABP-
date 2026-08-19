using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class GenericService<TEntity, TDto> : IGenericService<TDto>
            where TEntity : class
            where TDto : class
    {
        private readonly IGenericRepository<TEntity> _repository;
        private readonly IMapper _mapper;
        protected readonly ILogger<GenericService<TEntity, TDto>> _logger;

        public GenericService(IGenericRepository<TEntity> repository, IMapper mapper, ILogger<GenericService<TEntity, TDto>> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public virtual async Task<TDto?> AddAsync(TDto dto)
        {
            try
            {
                _logger.LogInformation("Adding new entity of type {EntityType}", typeof(TEntity).Name);
                TEntity entity = _mapper.Map<TEntity>(dto);
                
                TEntity? returnEntity = await _repository.AddAsync(entity);

                if (returnEntity == null)
                {
                    _logger.LogWarning("Failed to add entity of type {EntityType}", typeof(TEntity).Name);
                    return null;
                }

                _logger.LogInformation("Entity added successfully, returning mapped DTO");
                return _mapper.Map<TDto>(returnEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding entity of type {EntityType}", typeof(TEntity).Name);
                return null;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting entity of type {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                await _repository.DeleteAsync(id);
                _logger.LogInformation("Entity of type {EntityType} with ID: {Id} deleted successfully", typeof(TEntity).Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting entity of type {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                return false;
            }
        }

        public virtual async Task<List<TDto>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all entities of type {EntityType}", typeof(TEntity).Name);
                var listEntities = await _repository.GetAllListAsync();
                return _mapper.Map<List<TDto>>(listEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving all entities of type {EntityType}", typeof(TEntity).Name);
                return new List<TDto>();
            }
        }

        public virtual async Task<List<TDto>> GetAllWithIncludeAsync(List<string> properties)
        {
            try
            {
                _logger.LogInformation("Retrieving all entities of type {EntityType} with includes", typeof(TEntity).Name);
                var listEntities = await _repository.GetAllListWithInclude(properties);
                return _mapper.Map<List<TDto>>(listEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving all entities of type {EntityType} with includes", typeof(TEntity).Name);
                return new List<TDto>();
            }
        }

        public virtual async Task<TDto?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving entity of type {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                var entity = await _repository.GetByIdAsync(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("Entity of type {EntityType} with ID: {Id} not found", typeof(TEntity).Name, id);
                    return null;
                }

                return _mapper.Map<TDto>(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving entity of type {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                return null;
            }
        }

        public virtual async Task<TDto?> UpdateAsync(TDto dto, int id)
        {
            try
            {
                _logger.LogInformation("Updating entity of type {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                TEntity entity = _mapper.Map<TEntity>(dto);
                
                TEntity? returnEntity = await _repository.UpdateAsync(id, entity);

                if (returnEntity == null)
                {
                    _logger.LogWarning("Failed to update entity of type {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                    return null;
                }

                _logger.LogInformation("Entity of type {EntityType} with ID: {Id} updated successfully", typeof(TEntity).Name, id);
                return _mapper.Map<TDto>(returnEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating entity of type {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
                return null;
            }
        }
    }
}
