using System.Linq;
using ABP.Core.Application.Dtos.Commerces;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Queries.GetAllCommerces
{
    public class GetAllCommercesQuery : IRequest<IEnumerable<CommerceDto>> { public int PageNumber { get; set; } = 1; public int PageSize { get; set; } = 10; }

    public class GetAllCommercesQueryHandler : IRequestHandler<GetAllCommercesQuery, IEnumerable<CommerceDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IGenericRepository<ABP.Core.Domain.Entities.Commerce> _repository;
        private readonly IMapper _mapper;

        public GetAllCommercesQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.Commerce> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CommerceDto>> Handle(GetAllCommercesQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<CommerceDto>>(entities);
        }
    }
}

