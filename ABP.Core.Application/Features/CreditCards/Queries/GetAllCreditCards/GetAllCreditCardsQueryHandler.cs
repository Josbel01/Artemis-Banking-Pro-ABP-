using ABP.Core.Application.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.CreditCards.Queries.GetAllCreditCards
{
    public class GetAllCreditCardsQueryHandler : IRequestHandler<GetAllCreditCardsQuery, object>
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IAccountServiceWebApi _userManager;

        public GetAllCreditCardsQueryHandler(ICreditCardService creditCardService, IAccountServiceWebApi userManager)
        {
            _creditCardService = creditCardService;
            _userManager = userManager;
        }

        public async Task<object> Handle(GetAllCreditCardsQuery request, CancellationToken cancellationToken)
        {
            var cards = await _creditCardService.GetAllAsync();

            if (!string.IsNullOrEmpty(request.Identification))
            {
                var allUsers = await _userManager.GetAllUser(null);
                var user = allUsers.FirstOrDefault(u => u.DNI == request.Identification);
                if (user != null)
                {
                    cards = cards.Where(c => c.ClientId == user.Id).ToList();
                }
                else
                {
                    cards = new System.Collections.Generic.List<ABP.Core.Application.Dtos.CreditCards.CreditCardDto>();
                }
            }

            if (request.Status.ToLower() == "activa")
            {
                cards = cards.Where(c => c.Status == CreditCardStatus.Active).ToList();
            }
            else if (request.Status.ToLower() == "cancelada")
            {
                cards = cards.Where(c => c.Status == CreditCardStatus.Cancelled).ToList();
            }

            cards = cards.OrderByDescending(c => c.Id).ToList();

            var paged = cards.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new
            {
                page = request.Page,
                pageSize = request.PageSize,
                totalRecords = cards.Count,
                totalPages = cards.Count == 0 ? 1 : (int)Math.Ceiling(cards.Count / (double)request.PageSize),
                data = paged.Select(c => new {
                    id = c.Id.ToString(),
                    maskedCardNumber = $"************{c.CardNumber.Substring(c.CardNumber.Length - 4)}",
                    lastFourDigits = c.CardNumber.Substring(c.CardNumber.Length - 4),
                    clientId = c.ClientId,
                    clientFullName = "",
                    creditLimit = c.CreditLimit,
                    availableCredit = c.CreditLimit - c.CurrentDebt,
                    currentDebt = c.CurrentDebt,
                    expirationDate = c.ExpirationDate,
                    status = c.Status == CreditCardStatus.Active ? "Activa" : "Cancelada"
                })
            };
        }
    }
}

