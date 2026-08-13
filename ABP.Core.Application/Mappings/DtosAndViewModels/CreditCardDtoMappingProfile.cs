using AutoMapper;
using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.ViewModels.CreditCards;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class CreditCardDtoMappingProfile : Profile
    {
        public CreditCardDtoMappingProfile()
        {
            CreateMap<CreditCardViewModel, CreditCardDto>().ReverseMap();
            CreateMap<SaveCreditCardViewModel, CreditCardDto>().ReverseMap();
        }
    }
}
