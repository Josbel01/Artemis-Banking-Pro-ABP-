using System.Linq;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.LoanInstallments.Queries.GetAllLoanInstallments
{
    public class GetAllLoanInstallmentsQuery : IRequest<IEnumerable<LoanInstallmentDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
}

    public class GetAllLoanInstallmentsQueryHandler : IRequestHandler<GetAllLoanInstallmentsQuery, IEnumerable<LoanInstallmentDto>>
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

        public async Task<IEnumerable<LoanInstallmentDto>> Handle(GetAllLoanInstallmentsQuery query, CancellationToken cancellationToken)
        {
            var allEntities = await _repository.GetAllListAsync();
            var entities = allEntities.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize);
            return _mapper.Map<IEnumerable<LoanInstallmentDto>>(entities);
        }
    }
}

