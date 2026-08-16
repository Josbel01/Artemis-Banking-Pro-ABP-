using System.Linq;
using ABP.Core.Application.ViewModels.CardTransactions;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CardTransactions.Queries.GetAllCardTransactions
{
    public class GetAllCardTransactionsQuery : IRequest<IEnumerable<CardTransactionViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
}

    public class GetAllCardTransactionsQueryHandler : IRequestHandler<GetAllCardTransactionsQuery, IEnumerable<CardTransactionViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IGenericRepository<ABP.Core.Domain.Entities.CardTransaction> _repository;
        private readonly IMapper _mapper;

        public GetAllCardTransactionsQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.CardTransaction> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CardTransactionViewModel>> Handle(GetAllCardTransactionsQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<CardTransactionViewModel>>(entities);
        }
    }
}

