using AutoMapper;
using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.ViewModels.User;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class UserDtoMappingProfile : Profile
    {
        public UserDtoMappingProfile()
        {
            CreateMap<UserViewModel, UserDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DNI, opt => opt.MapFrom(src => src.Identification))
                .ReverseMap()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Identification, opt => opt.MapFrom(src => src.DNI));
        }
    }
}
