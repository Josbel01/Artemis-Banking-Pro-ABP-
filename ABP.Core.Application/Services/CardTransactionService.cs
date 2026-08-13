using ABP.Core.Application.Dtos.CardTransactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class CardTransactionService : GenericService<CardTransaction, CardTransactionDto>, ICardTransactionService
    {
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly IMapper _mapper;

        public CardTransactionService(ICardTransactionRepository cardTransactionRepository, IMapper mapper) 
            : base(cardTransactionRepository, mapper)
        {
            _cardTransactionRepository = cardTransactionRepository;
            _mapper = mapper;
        }

        public async Task<List<CardTransactionDto>> GetAllByCreditCardIdAsync(int creditCardId)
        {
            var transactions = await _cardTransactionRepository.GetAllListAsync();
            var cardTransactions = transactions.Where(t => t.CreditCardId == creditCardId).ToList();
            return _mapper.Map<List<CardTransactionDto>>(cardTransactions);
        }
    }
}
