using ABP.Core.Application.Dtos.CardTransactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class CardTransactionService : GenericService<CardTransaction, CardTransactionDto>, ICardTransactionService
    {
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CardTransactionService> _logger;

        public CardTransactionService(ICardTransactionRepository cardTransactionRepository, IMapper mapper, ILoggerFactory loggerFactory) 
            : base(cardTransactionRepository, mapper, loggerFactory.CreateLogger<GenericService<CardTransaction, CardTransactionDto>>())
        {
            _cardTransactionRepository = cardTransactionRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<CardTransactionService>();
        }

        public async Task<List<CardTransactionDto>> GetAllByCreditCardIdAsync(int creditCardId)
        {
            _logger.LogInformation("Retrieving all transactions for credit card ID: {CreditCardId}", creditCardId);
            var transactions = await _cardTransactionRepository.GetAllListAsync();
            var cardTransactions = transactions.Where(t => t.CreditCardId == creditCardId).ToList();
            _logger.LogInformation("Found {Count} transactions for credit card ID: {CreditCardId}", cardTransactions.Count, creditCardId);
            return _mapper.Map<List<CardTransactionDto>>(cardTransactions);
        }
    }
}
