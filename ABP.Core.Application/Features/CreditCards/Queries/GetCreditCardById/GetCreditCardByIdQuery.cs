using ABP.Core.Application.ViewModels.CreditCards;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CreditCards.Queries.GetCreditCardById
{
    public class GetCreditCardByIdQuery : IRequest<CreditCardViewModel>
    {
        public int Id { get; set; }
    }

    public class GetCreditCardByIdQueryHandler : IRequestHandler<GetCreditCardByIdQuery, CreditCardViewModel>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.CreditCard> _repository;
        private readonly IMapper _mapper;

        public GetCreditCardByIdQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.CreditCard> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CreditCardViewModel> Handle(GetCreditCardByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(query.Id);
            if (entity == null) throw new Exception("CreditCard not found with this id");

            return _mapper.Map<CreditCardViewModel>(entity);
        }
    }
}
