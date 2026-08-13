using AutoMapper;
using ABP.Core.Application.Dtos.Loans;
using ABP.Core.Application.ViewModels.Loans;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class LoanDtoMappingProfile : Profile
    {
        public LoanDtoMappingProfile()
        {
            CreateMap<LoanViewModel, LoanDto>().ReverseMap();
            CreateMap<SaveLoanViewModel, LoanDto>().ReverseMap();
            CreateMap<LoanInstallmentViewModel, LoanInstallmentDto>().ReverseMap();
        }
    }
}
