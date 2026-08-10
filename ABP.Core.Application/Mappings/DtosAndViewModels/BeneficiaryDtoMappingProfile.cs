using AutoMapper;
using ABP.Core.Application.Dtos.Beneficiaries;
using ABP.Core.Application.ViewModels.Beneficiaries;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class BeneficiaryDtoMappingProfile : Profile
    {
        public BeneficiaryDtoMappingProfile()
        {
            CreateMap<BeneficiaryDto, BeneficiaryViewModel>()
                .ReverseMap();

            CreateMap<SaveBeneficiaryDto, SaveBeneficiaryViewModel>()
                .ReverseMap();

            CreateMap<SaveBeneficiaryViewModel, BeneficiaryDto>()
                .ReverseMap();
        }
    }
}
