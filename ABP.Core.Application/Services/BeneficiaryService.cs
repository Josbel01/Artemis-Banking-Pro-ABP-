using ABP.Core.Application.Dtos.Beneficiaries;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class BeneficiaryService : GenericService<Beneficiary, BeneficiaryDto>, IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly IMapper _mapper;

        public BeneficiaryService(IBeneficiaryRepository beneficiaryRepository, IMapper mapper) 
            : base(beneficiaryRepository, mapper)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _mapper = mapper;
        }

        public async Task<List<BeneficiaryDto>> GetAllByClientIdAsync(string clientId)
        {
            var allBeneficiaries = await _beneficiaryRepository.GetAllListAsync();
            
            var clientBeneficiaries = allBeneficiaries
                .Where(b => b.ClientId == clientId)
                .ToList();

            return _mapper.Map<List<BeneficiaryDto>>(clientBeneficiaries);
        }
    }
}
