using AutoMapper;
using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Domain.Entities;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class SavingAccountMappingProfile : Profile
    {
        public SavingAccountMappingProfile()
        {
            CreateMap<SavingAccount, SavingAccountDto>()
                .ReverseMap();
        }
    }
}
