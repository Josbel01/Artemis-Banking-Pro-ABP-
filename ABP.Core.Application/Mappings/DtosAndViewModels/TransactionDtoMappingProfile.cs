using AutoMapper;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.ViewModels.Transactions;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class TransactionDtoMappingProfile : Profile
    {
        public TransactionDtoMappingProfile()
        {
            CreateMap<TransactionDto, TransactionViewModel>()
                .ReverseMap();

            CreateMap<SaveTransferDto, SaveTransferViewModel>()
                .ReverseMap();

            CreateMap<SaveCashAdvanceDto, SaveCashAdvanceViewModel>()
                .ReverseMap();
        }
    }
}
