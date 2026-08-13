using ABP.Core.Application.Dtos.CreditCards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface ICreditCardService : IGenericService<CreditCardDto>
    {
        Task<List<CreditCardDto>> GetAllByClientIdAsync(string clientId);
        Task<CreditCardDto?> GetByCardNumberAsync(string cardNumber);
    }
}
