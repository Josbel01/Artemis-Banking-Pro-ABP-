using ABP.Core.Application.Dtos.Beneficiaries;
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
    public class BeneficiaryService : GenericService<Beneficiary, BeneficiaryDto>, IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BeneficiaryService> _logger;

        public BeneficiaryService(IBeneficiaryRepository beneficiaryRepository, IMapper mapper, ILoggerFactory loggerFactory) 
            : base(beneficiaryRepository, mapper, loggerFactory.CreateLogger<GenericService<Beneficiary, BeneficiaryDto>>())
        {
            _beneficiaryRepository = beneficiaryRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<BeneficiaryService>();
        }

        public async Task<List<BeneficiaryDto>> GetAllByClientIdAsync(string clientId)
        {
            _logger.LogInformation("Retrieving all beneficiaries for client ID: {ClientId}", clientId);
            var allBeneficiaries = await _beneficiaryRepository.GetAllListAsync();
            
            var clientBeneficiaries = allBeneficiaries
                .Where(b => b.ClientId == clientId)
                .ToList();

            _logger.LogInformation("Found {Count} beneficiaries for client ID: {ClientId}", clientBeneficiaries.Count, clientId);
            return _mapper.Map<List<BeneficiaryDto>>(clientBeneficiaries);
        }
    }
}
