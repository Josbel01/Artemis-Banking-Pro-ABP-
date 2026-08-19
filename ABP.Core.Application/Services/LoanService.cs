using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LoanService> _logger;

        public LoanService(ILoanRepository loanRepository, IMapper mapper, ILoggerFactory loggerFactory) 
            : base(loanRepository, mapper, loggerFactory.CreateLogger<GenericService<Loan, LoanDto>>())
        {
            _loanRepository = loanRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<LoanService>();
        }

        public async Task<List<LoanDto>> GetAllByClientIdAsync(string clientId)
        {
            _logger.LogInformation("Retrieving all loans for client ID: {ClientId}", clientId);
            var loans = await _loanRepository.GetAllListAsync();
            var clientLoans = loans.Where(l => l.ClientId == clientId).ToList();
            _logger.LogInformation("Found {Count} loans for client ID: {ClientId}", clientLoans.Count, clientId);
            return _mapper.Map<List<LoanDto>>(clientLoans);
        }

        public async Task<LoanDto?> GetByLoanNumberAsync(string loanNumber)
        {
            _logger.LogInformation("Retrieving loan by loan number");
            var loans = await _loanRepository.GetAllListAsync();
            var loan = loans.FirstOrDefault(l => l.LoanNumber == loanNumber);
            
            if (loan == null)
            {
                _logger.LogWarning("Loan not found");
                return null;
            }

            _logger.LogInformation("Loan found");
            return _mapper.Map<LoanDto>(loan);
        }
    }
}
