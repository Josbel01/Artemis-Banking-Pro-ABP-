using System.Linq;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Transactions.Queries.GetAllTransactions
{
    public class GetAllTransactionsQuery : IRequest<IEnumerable<TransactionDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
}

    public class GetAllTransactionsQueryHandler : IRequestHandler<GetAllTransactionsQuery, IEnumerable<TransactionDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IGenericRepository<ABP.Core.Domain.Entities.Transaction> _repository;
        private readonly IMapper _mapper;

        public GetAllTransactionsQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.Transaction> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TransactionDto>> Handle(GetAllTransactionsQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<TransactionDto>>(entities);
        }
    }
}

