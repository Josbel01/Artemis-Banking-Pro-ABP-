using AutoMapper;
using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Application.ViewModels.SavingAccounts;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class SavingAccountDtoMappingProfile : Profile
    {
        public SavingAccountDtoMappingProfile()
        {
            CreateMap<SavingAccountViewModel, SavingAccountDto>().ReverseMap();
            CreateMap<SaveSavingAccountViewModel, SavingAccountDto>().ReverseMap();
        }
    }
}
