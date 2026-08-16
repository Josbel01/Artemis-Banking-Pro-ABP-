using System.Linq;
using ABP.Core.Application.ViewModels.Loans;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.LoanInstallments.Queries.GetAllLoanInstallments
{
    public class GetAllLoanInstallmentsQuery : IRequest<IEnumerable<LoanInstallmentViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
}

    public class GetAllLoanInstallmentsQueryHandler : IRequestHandler<GetAllLoanInstallmentsQuery, IEnumerable<LoanInstallmentViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
private readonly IGenericRepository<ABP.Core.Domain.Entities.LoanInstallment> _repository;
        private readonly IMapper _mapper;

        public GetAllLoanInstallmentsQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.LoanInstallment> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LoanInstallmentViewModel>> Handle(GetAllLoanInstallmentsQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<LoanInstallmentViewModel>>(entities);
        }
    }
}

