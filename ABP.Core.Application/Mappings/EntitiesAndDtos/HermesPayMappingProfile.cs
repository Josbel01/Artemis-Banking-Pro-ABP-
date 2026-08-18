using ABP.Core.Application.Features.HermesPay.Queries.GetPaymentTransactions;
using ABP.Core.Domain.Entities;
using AutoMapper;

namespace ABP.Core.Application.Mappings.EntitiesAndDtos
{
    public class HermesPayMappingProfile : Profile
    {
        public HermesPayMappingProfile()
        {
            CreateMap<Transaction, PaymentTransactionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.CardLastFourDigits, opt => opt.MapFrom(src => src.Origin))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 
                    src.Status == Domain.Common.Enums.TransactionStatus.Approved ? "APROBADO" : "RECHAZADO"));
        }
    }
}
