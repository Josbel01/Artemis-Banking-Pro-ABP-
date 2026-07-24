using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class GenericService<TEntity, TDto> : IGenericService<TDto>
            where TEntity : class
            where TDto : class
    {
        private readonly IGenericRepository<TEntity> _repository;
        private readonly IMapper _mapper;

        public GenericService(IGenericRepository<TEntity> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<TDto?> AddAsync(TDto dto)
        {
            try
            {
                TEntity T = _mapper.Map<TEntity>(dto);
                TEntity? returnEntity = await _repository.AddAsync(T);
                if (returnEntity == null)
                {
                    return null;
                }
                return _mapper.Map<TDto>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual async Task<List<TDto>> GetAllAsync()
        {
            try
            {
                var listEntities = await _repository.GetAllListAsync();
                var listEntityDtos = _mapper.Map<List<TDto>>(listEntities);

                return listEntityDtos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        public virtual async Task<List<TDto>> GetAllWithIncludeAsync(List<string> properties)
        {
            try
            {
                var listEntities = await _repository.GetAllListWithInclude(properties);
                return _mapper.Map<List<TDto>>(listEntities);
            }
            catch (Exception)
            {
                return [];
            }
        }

        public virtual async Task<TDto?> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return null;
                }

                TDto dto = _mapper.Map<TDto>(entity);
                return dto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<TDto?> UpdateAsync(TDto dto, int id)
        {
            try
            {
                TEntity entity = _mapper.Map<TEntity>(dto);
                TEntity? returnEntity = await _repository.UpdateAsync(id, entity);
                if (returnEntity == null)
                {
                    return null;
                }

                return _mapper.Map<TDto>(returnEntity);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }


}
