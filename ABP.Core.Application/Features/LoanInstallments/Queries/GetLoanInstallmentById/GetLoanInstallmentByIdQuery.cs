using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.LoanInstallments.Queries.GetLoanInstallmentById
{
    public class GetLoanInstallmentByIdQuery : IRequest<LoanInstallmentDto>
    {
        public int Id { get; set; }
    }

    public class GetLoanInstallmentByIdQueryHandler : IRequestHandler<GetLoanInstallmentByIdQuery, LoanInstallmentDto>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.LoanInstallment> _repository;
        private readonly IMapper _mapper;

        public GetLoanInstallmentByIdQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.LoanInstallment> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<LoanInstallmentDto> Handle(GetLoanInstallmentByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(query.Id);
            if (entity == null) throw new Exception("LoanInstallment not found with this id");

            return _mapper.Map<LoanInstallmentDto>(entity);
        }
    }
}
