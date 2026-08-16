using ABP.Core.Application.ViewModels.Loans;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQuery : IRequest<LoanViewModel>
    {
        public int Id { get; set; }
    }

    public class GetLoanByIdQueryHandler : IRequestHandler<GetLoanByIdQuery, LoanViewModel>
    {
        private readonly IGenericRepository<ABP.Core.Domain.Entities.Loan> _repository;
        private readonly IMapper _mapper;

        public GetLoanByIdQueryHandler(IGenericRepository<ABP.Core.Domain.Entities.Loan> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<LoanViewModel> Handle(GetLoanByIdQuery query, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(query.Id);
            if (entity == null) throw new Exception("Loan not found with this id");

            return _mapper.Map<LoanViewModel>(entity);
        }
    }
}
