using ABP.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Domain.Interfaces
{
    public interface ICardTransactionRepository : IGenericRepository<CardTransaction>
    {
        Task<List<CardTransaction>> GetAllByCreditCardIdAsync(int creditCardId);
        Task<List<CardTransaction>> GetAllByCommerceIdAsync(int commerceId);
    }
}
