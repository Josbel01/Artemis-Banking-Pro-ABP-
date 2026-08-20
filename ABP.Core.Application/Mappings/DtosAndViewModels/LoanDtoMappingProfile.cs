using AutoMapper;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.ViewModels.Loans;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class LoanDtoMappingProfile : Profile
    {
        public LoanDtoMappingProfile()
        {
            CreateMap<LoanDto, LoanViewModel>()
                .ForMember(dest => dest.PrincipalAmount, opt => opt.MapFrom(src => src.AmountApproved))
                .ForMember(dest => dest.RemainingDebt, opt => opt.MapFrom(src => src.AmountPending))
                .ReverseMap()
                .ForMember(dest => dest.AmountApproved, opt => opt.MapFrom(src => src.PrincipalAmount))
                .ForMember(dest => dest.AmountPending, opt => opt.MapFrom(src => src.RemainingDebt));
            CreateMap<SaveLoanViewModel, LoanDto>().ReverseMap();
            CreateMap<LoanInstallmentViewModel, LoanInstallmentDto>().ReverseMap();
        }
    }
}
