using ABP.Core.Application.Dtos.HermesPay;
using ABP.Core.Application.Exceptions;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.HermesPay.Queries.GetPaymentTransactions
{
    public class GetPaymentTransactionsQueryHandler : IRequestHandler<GetPaymentTransactionsQuery, PaymentTransactionResponse>
    {
        private readonly ICommerceRepository _commerceRepository;
        private readonly ICardTransactionRepository _cardTransactionRepository;
        private readonly ICreditCardRepository _creditCardRepository;

        public GetPaymentTransactionsQueryHandler(
            ICommerceRepository commerceRepository,
            ICardTransactionRepository cardTransactionRepository,
            ICreditCardRepository creditCardRepository)
        {
            _commerceRepository = commerceRepository;
            _cardTransactionRepository = cardTransactionRepository;
            _creditCardRepository = creditCardRepository;
        }

        public async Task<PaymentTransactionResponse> Handle(GetPaymentTransactionsQuery request, CancellationToken cancellationToken)
        {
            var commerce = await ResolveCommerceAsync(request);

            var allCardTransactions = await _cardTransactionRepository.GetAllByCommerceIdAsync(commerce.Id);
            var allCards = await _creditCardRepository.GetAllListAsync();

            var query = allCardTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            int totalRecords = query.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            var pagedData = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ct =>
                {
                    var card = allCards.FirstOrDefault(c => c.Id == ct.CreditCardId);
                    var lastFour = card != null && card.CardNumber.Length >= 4
                        ? card.CardNumber[^4..]
                        : "****";

                    return new PaymentTransactionDto
                    {
                        Id = ct.Id.ToString(),
                        TransactionDate = ct.TransactionDate,
                        Amount = ct.Amount,
                        CardLastFourDigits = lastFour,
                        Status = ct.Status == TransactionStatus.Approved ? "APROBADO" : "RECHAZADO"
                    };
                })
                .ToList();

            return new PaymentTransactionResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CommerceId = commerce.Id,
                CommerceName = commerce.Name,
                Data = pagedData
            };
        }

        private async Task<ABP.Core.Domain.Entities.Commerce> ResolveCommerceAsync(GetPaymentTransactionsQuery request)
        {
            ABP.Core.Domain.Entities.Commerce commerce;

            if (!string.IsNullOrEmpty(request.CommerceUserId))
                commerce = await _commerceRepository.GetByUserIdAsync(request.CommerceUserId);
            else
                commerce = await _commerceRepository.GetByIdAsync(request.CommerceId);

            if (commerce == null)
                throw new ApiException("El comercio no existe.");

            if (!commerce.IsActive)
                throw new ApiException("El comercio no está activo.");

            return commerce;
        }
    }
}
