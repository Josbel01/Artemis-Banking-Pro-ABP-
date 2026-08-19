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
    public class LoanInstallmentService : GenericService<LoanInstallment, LoanInstallmentDto>, ILoanInstallmentService
    {
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LoanInstallmentService> _logger;

        public LoanInstallmentService(ILoanInstallmentRepository loanInstallmentRepository, IMapper mapper, ILoggerFactory loggerFactory) 
            : base(loanInstallmentRepository, mapper, loggerFactory.CreateLogger<GenericService<LoanInstallment, LoanInstallmentDto>>())
        {
            _loanInstallmentRepository = loanInstallmentRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<LoanInstallmentService>();
        }

        public async Task<List<LoanInstallmentDto>> GetAllByLoanIdAsync(int loanId)
        {
            _logger.LogInformation("Retrieving all installments for loan ID: {LoanId}", loanId);
            var installments = await _loanInstallmentRepository.GetAllListAsync();
            var loanInstallments = installments.Where(i => i.LoanId == loanId).ToList();
            _logger.LogInformation("Found {Count} installments for loan ID: {LoanId}", loanInstallments.Count, loanId);
            return _mapper.Map<List<LoanInstallmentDto>>(loanInstallments);
        }
    }
}
