using AutoMapper;
using ABP.Core.Application.Dtos.CardTransactions;
using ABP.Core.Domain.Entities;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class CardTransactionMappingProfile : Profile
    {
        public CardTransactionMappingProfile()
        {
            CreateMap<CardTransaction, CardTransactionDto>()
                .ReverseMap();
        }
    }
}
