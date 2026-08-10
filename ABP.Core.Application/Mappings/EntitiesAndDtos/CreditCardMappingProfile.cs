using AutoMapper;
using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Domain.Entities;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class CreditCardMappingProfile : Profile
    {
        public CreditCardMappingProfile()
        {
            CreateMap<CreditCard, CreditCardDto>()
                .ReverseMap();
        }
    }
}
