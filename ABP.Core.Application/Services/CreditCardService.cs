using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class CreditCardService : GenericService<CreditCard, CreditCardDto>, ICreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly IMapper _mapper;

        public CreditCardService(ICreditCardRepository creditCardRepository, IMapper mapper) 
            : base(creditCardRepository, mapper)
        {
            _creditCardRepository = creditCardRepository;
            _mapper = mapper;
        }

        public async Task<List<CreditCardDto>> GetAllByClientIdAsync(string clientId)
        {
            var cards = await _creditCardRepository.GetAllListAsync();
            var clientCards = cards.Where(c => c.ClientId == clientId).ToList();
            return _mapper.Map<List<CreditCardDto>>(clientCards);
        }

        public async Task<CreditCardDto?> GetByCardNumberAsync(string cardNumber)
        {
            var cards = await _creditCardRepository.GetAllListAsync();
            var card = cards.FirstOrDefault(c => c.CardNumber == cardNumber);
            return card == null ? null : _mapper.Map<CreditCardDto>(card);
        }
    }
}
