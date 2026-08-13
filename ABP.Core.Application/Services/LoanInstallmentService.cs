using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class LoanInstallmentService : GenericService<LoanInstallment, LoanInstallmentDto>, ILoanInstallmentService
    {
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly IMapper _mapper;

        public LoanInstallmentService(ILoanInstallmentRepository loanInstallmentRepository, IMapper mapper) 
            : base(loanInstallmentRepository, mapper)
        {
            _loanInstallmentRepository = loanInstallmentRepository;
            _mapper = mapper;
        }

        public async Task<List<LoanInstallmentDto>> GetAllByLoanIdAsync(int loanId)
        {
            var installments = await _loanInstallmentRepository.GetAllListAsync();
            var loanInstallments = installments.Where(i => i.LoanId == loanId).ToList();
            return _mapper.Map<List<LoanInstallmentDto>>(loanInstallments);
        }
    }
}
