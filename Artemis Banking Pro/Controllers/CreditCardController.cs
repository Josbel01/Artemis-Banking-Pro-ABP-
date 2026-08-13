using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.CreditCards;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IMapper _mapper;

        public CreditCardController(ICreditCardService creditCardService, IMapper mapper)
        {
            _creditCardService = creditCardService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var cards = await _creditCardService.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<CreditCardViewModel>>(cards);
            return View(viewModels);
        }
    }
}
