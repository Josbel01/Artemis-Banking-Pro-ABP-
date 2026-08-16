using System.Linq;
using ABP.Core.Application.ViewModels.Loans;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Loans.Queries.GetAllLoans
{
    public class GetAllLoansQuery : IRequest<IEnumerable<LoanViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
}

    public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, IEnumerable<LoanViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IGenericRepository<ABP.Core.Domain.Entities.Loan> _repository;
        private readonly IMapper _mapper;

        public GetAllLoansQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.Loan> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LoanViewModel>> Handle(GetAllLoansQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<LoanViewModel>>(entities);
        }
    }
}

