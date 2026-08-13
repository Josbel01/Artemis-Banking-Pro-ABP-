using AutoMapper;
using ABP.Core.Application.Dtos.CardTransactions;
using ABP.Core.Application.ViewModels.CardTransactions;

namespace ABP.Core.Application.Mappings.DtosAndViewModels
{
    public class CardTransactionDtoMappingProfile : Profile
    {
        public CardTransactionDtoMappingProfile()
        {
            CreateMap<CardTransactionViewModel, CardTransactionDto>().ReverseMap();
        }
    }
}
