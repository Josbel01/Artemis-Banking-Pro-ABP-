using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.CardTransactions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize]
    public class CardTransactionController : Controller
    {
        private readonly ICardTransactionService _cardTransactionService;
        private readonly IMapper _mapper;

        public CardTransactionController(ICardTransactionService cardTransactionService, IMapper mapper)
        {
            _cardTransactionService = cardTransactionService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(int creditCardId)
        {
            if (creditCardId == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            
            var transactions = await _cardTransactionService.GetAllByCreditCardIdAsync(creditCardId);
            var viewModels = _mapper.Map<IEnumerable<CardTransactionViewModel>>(transactions);
            
            ViewBag.CreditCardId = creditCardId;
            return View(viewModels);
        }
    }
}
