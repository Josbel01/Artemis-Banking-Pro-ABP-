using AutoMapper;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Domain.Entities;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<Transaction, TransactionDto>()
                .ReverseMap();
        }
    }
}
