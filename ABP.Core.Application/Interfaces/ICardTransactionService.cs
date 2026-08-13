using ABP.Core.Application.Dtos.CardTransactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface ICardTransactionService : IGenericService<CardTransactionDto>
    {
        Task<List<CardTransactionDto>> GetAllByCreditCardIdAsync(int creditCardId);
    }
}
