using AutoMapper;
using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.ViewModels.CreditCards;
using System.Globalization;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class CreditCardDtoMappingProfile : Profile
    {
        public CreditCardDtoMappingProfile()
        {
            CreateMap<CreditCardViewModel, CreditCardDto>()
                .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => src.ExpirationDate.ToString("MM/yy")))
                .ReverseMap()
                .ForMember(dest => dest.ExpirationDate, opt => opt.MapFrom(src => ParseExpirationDate(src.ExpirationDate)));
            CreateMap<SaveCreditCardViewModel, CreditCardDto>().ReverseMap();
        }

        private static DateTime ParseExpirationDate(string dateStr)
        {
            if (DateTime.TryParseExact(dateStr, "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            return DateTime.MinValue;
        }
    }
}
