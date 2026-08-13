using AutoMapper;
using ABP.Core.Application.Dtos.Cashier;
using ABP.Core.Application.ViewModels.Cashier;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class CashierDtoMappingProfile : Profile
    {
        public CashierDtoMappingProfile()
        {
            CreateMap<DepositViewModel, CashierDepositDto>().ReverseMap();
            CreateMap<WithdrawalViewModel, CashierWithdrawalDto>().ReverseMap();
            CreateMap<CreditCardPaymentViewModel, CashierCreditCardPaymentDto>().ReverseMap();
            CreateMap<LoanPaymentViewModel, CashierLoanPaymentDto>().ReverseMap();
            CreateMap<CashierTransferViewModel, CashierTransferDto>().ReverseMap();
            CreateMap<CashierHomeViewModel, DailyIndicatorsDto>().ReverseMap();
        }
    }
}
