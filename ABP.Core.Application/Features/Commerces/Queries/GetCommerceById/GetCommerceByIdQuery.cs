using ABP.Core.Application.Dtos.Commerces;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Queries.GetCommerceById
{
    public class GetCommerceByIdQuery : IRequest<CommerceDto>
    {
        public int Id { get; set; }
    }

    public class GetCommerceByIdQueryHandler : IRequestHandler<GetCommerceByIdQuery, CommerceDto>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.Commerce> _repository;
        private readonly IMapper _mapper;

        public GetCommerceByIdQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.Commerce> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CommerceDto> Handle(GetCommerceByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(query.Id);
            if (entity == null) throw new Exception("Commerce not found");
            return _mapper.Map<CommerceDto>(entity);
        }
    }
}
