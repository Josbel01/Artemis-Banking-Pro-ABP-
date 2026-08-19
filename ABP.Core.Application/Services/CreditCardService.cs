using ABP.Core.Application.Dtos.CreditCards;
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
    public class CreditCardService : GenericService<CreditCard, CreditCardDto>, ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreditCardService> _logger;

        public CreditCardService(ICreditCardRepository creditCardRepository, IMapper mapper, ILoggerFactory loggerFactory) 
            : base(creditCardRepository, mapper, loggerFactory.CreateLogger<GenericService<CreditCard, CreditCardDto>>())
        {
            _creditCardRepository = creditCardRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<CreditCardService>();
        }

        public async Task<List<CreditCardDto>> GetAllByClientIdAsync(string clientId)
        {
            _logger.LogInformation("Retrieving all credit cards for client ID: {ClientId}", clientId);
            var cards = await _creditCardRepository.GetAllListAsync();
            var clientCards = cards.Where(c => c.ClientId == clientId).ToList();
            _logger.LogInformation("Found {Count} credit cards for client ID: {ClientId}", clientCards.Count, clientId);
            return _mapper.Map<List<CreditCardDto>>(clientCards);
        }

        public async Task<CreditCardDto?> GetByCardNumberAsync(string cardNumber)
        {
            _logger.LogInformation("Retrieving credit card by card number");
            var cards = await _creditCardRepository.GetAllListAsync();
            var card = cards.FirstOrDefault(c => c.CardNumber == cardNumber);
            
            if (card == null)
            {
                _logger.LogWarning("Credit card not found");
                return null;
            }

            _logger.LogInformation("Credit card found");
            return _mapper.Map<CreditCardDto>(card);
        }
    }
}
