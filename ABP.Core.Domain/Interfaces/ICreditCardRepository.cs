using ABP.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<CreditCard?> GetByCardNumberAsync(string cardNumber);
        Task<List<CreditCard>> GetAllByClientIdAsync(string clientId);
    }
}
