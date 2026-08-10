using AutoMapper;
using ABP.Core.Application.Dtos;
using ABP.Core.Domain.Common;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class BasicMappingProfile : Profile
    {
        public BasicMappingProfile()
        {
            CreateMap(typeof(BasicDto<>), typeof(BasicEntity<>))
                .ForMember("CreatedAt", opt => opt.MapFrom("Created"))
                .ForMember("UpdatedAt", opt => opt.MapFrom("Updated"))
                .ReverseMap()
                .ForMember("Created", opt => opt.MapFrom("CreatedAt"))
                .ForMember("Updated", opt => opt.MapFrom("UpdatedAt"));
        }
    }
}
