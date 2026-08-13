using AutoMapper;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Domain.Entities;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class LoanInstallmentMappingProfile : Profile
    {
        public LoanInstallmentMappingProfile()
        {
            CreateMap<LoanInstallment, LoanInstallmentDto>()
                .ReverseMap();
        }
    }
}
