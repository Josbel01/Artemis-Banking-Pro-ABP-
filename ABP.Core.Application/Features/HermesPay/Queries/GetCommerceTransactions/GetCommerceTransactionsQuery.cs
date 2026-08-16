using ABP.Core.Application.Dtos.CardTransactions;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.HermesPay.Queries.GetCommerceTransactions
{
    public class GetCommerceTransactionsQuery : IRequest<IEnumerable<CardTransactionDto>>
    {
        public int CommerceId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetCommerceTransactionsQueryHandler : IRequestHandler<GetCommerceTransactionsQuery, IEnumerable<CardTransactionDto>>
    {
        private readonly ICardTransactionRepository _repository;
        private readonly IMapper _mapper;

        public GetCommerceTransactionsQueryHandler(ICardTransactionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CardTransactionDto>> Handle(GetCommerceTransactionsQuery query, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllListAsync();
            var paginated = entities.Where(x => x.CommerceId == query.CommerceId)
                                    .Skip((query.PageNumber - 1) * query.PageSize)
                                    .Take(query.PageSize);
            return _mapper.Map<IEnumerable<CardTransactionDto>>(paginated);
        }
    }
}
