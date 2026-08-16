using System.Linq;
using ABP.Core.Application.ViewModels.CreditCards;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.CreditCards.Queries.GetAllCreditCards
{
    public class GetAllCreditCardsQuery : IRequest<IEnumerable<CreditCardViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
}

    public class GetAllCreditCardsQueryHandler : IRequestHandler<GetAllCreditCardsQuery, IEnumerable<CreditCardViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IGenericRepository<ABP.Core.Domain.Entities.CreditCard> _repository;
        private readonly IMapper _mapper;

        public GetAllCreditCardsQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.CreditCard> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CreditCardViewModel>> Handle(GetAllCreditCardsQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<CreditCardViewModel>>(entities);
        }
    }
}

