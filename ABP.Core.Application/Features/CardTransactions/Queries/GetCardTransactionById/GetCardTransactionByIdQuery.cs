using ABP.Core.Application.ViewModels.CardTransactions;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CardTransactions.Queries.GetCardTransactionById
{
    public class GetCardTransactionByIdQuery : IRequest<CardTransactionViewModel>
    {
        public int Id { get; set; }
    }

    public class GetCardTransactionByIdQueryHandler : IRequestHandler<GetCardTransactionByIdQuery, CardTransactionViewModel>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.CardTransaction> _repository;
        private readonly IMapper _mapper;

        public GetCardTransactionByIdQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.CardTransaction> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CardTransactionViewModel> Handle(GetCardTransactionByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(query.Id);
            if (entity == null) throw new Exception("CardTransaction not found with this id");

            return _mapper.Map<CardTransactionViewModel>(entity);
        }
    }
}
