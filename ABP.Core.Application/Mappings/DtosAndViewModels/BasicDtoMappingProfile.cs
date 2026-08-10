using AutoMapper;
using ABP.Core.Application.Dtos;
using ABP.Core.Application.ViewModels;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class BasicDtoMappingProfile : Profile
    {
        public BasicDtoMappingProfile()
        {
            CreateMap(typeof(BasicDto<>), typeof(BasicViewModel<>)).ReverseMap();
        }
    }
}
