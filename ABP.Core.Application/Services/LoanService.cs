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
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;

        public LoanService(ILoanRepository loanRepository, IMapper mapper) 
            : base(loanRepository, mapper)
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
        }

        public async Task<List<LoanDto>> GetAllByClientIdAsync(string clientId)
        {
            var loans = await _loanRepository.GetAllListAsync();
            var clientLoans = loans.Where(l => l.ClientId == clientId).ToList();
            return _mapper.Map<List<LoanDto>>(clientLoans);
        }

        public async Task<LoanDto?> GetByLoanNumberAsync(string loanNumber)
        {
            var loans = await _loanRepository.GetAllListAsync();
            var loan = loans.FirstOrDefault(l => l.LoanNumber == loanNumber);
            return loan == null ? null : _mapper.Map<LoanDto>(loan);
        }
    }
}
