using AutoMapper;
using ABP.Core.Application.Dtos.Beneficiaries;
using ABP.Core.Domain.Entities;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class BeneficiaryMappingProfile : Profile
    {
        public BeneficiaryMappingProfile()
        {
            CreateMap<Beneficiary, BeneficiaryDto>()
                .ReverseMap();

            CreateMap<Beneficiary, SaveBeneficiaryDto>()
                .ReverseMap();
        }
    }
}
