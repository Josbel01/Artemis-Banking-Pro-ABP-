using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Transactions.Queries.GetTransactionById
{
    public class GetTransactionByIdQuery : IRequest<TransactionDto>
    {
        public int Id { get; set; }
    }

    public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, TransactionDto>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.Transaction> _repository;
        private readonly IMapper _mapper;

        public GetTransactionByIdQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.Transaction> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<TransactionDto> Handle(GetTransactionByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(query.Id);
            if (entity == null) throw new Exception("Transaction not found with this id");

            return _mapper.Map<TransactionDto>(entity);
        }
    }
}
