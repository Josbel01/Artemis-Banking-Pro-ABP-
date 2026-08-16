using System.Linq;
using ABP.Core.Application.ViewModels.SavingAccounts;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.SavingAccounts.Queries.GetAllSavingAccounts
{
    public class GetAllSavingAccountsQuery : IRequest<IEnumerable<SavingAccountViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
}

    public class GetAllSavingAccountsQueryHandler : IRequestHandler<GetAllSavingAccountsQuery, IEnumerable<SavingAccountViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IGenericRepository<ABP.Core.Domain.Entities.SavingAccount> _repository;
        private readonly IMapper _mapper;

        public GetAllSavingAccountsQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.SavingAccount> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SavingAccountViewModel>> Handle(GetAllSavingAccountsQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<SavingAccountViewModel>>(entities);
        }
    }
}

