using ABP.Core.Application.Exceptions;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace ABP.Core.Application.Features.HermesPay.Queries.GetPaymentTransactions
{
    public class PaymentTransactionDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string CardLastFourDigits { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentTransactionResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CommerceId { get; set; }
        public string CommerceName { get; set; } = string.Empty;
        public List<PaymentTransactionDto> Data { get; set; } = new List<PaymentTransactionDto>();
    }

    public class GetPaymentTransactionsQuery : IRequest<PaymentTransactionResponse>
    {
        public int CommerceId { get; set; }
        public string? CommerceUserId { get; set; }
        
        [SwaggerParameter(Description = "The page number to retrieve", Required = false)]
        public int Page { get; set; } = 1;
        
        [SwaggerParameter(Description = "The number of records per page", Required = false)]
        public int PageSize { get; set; } = 20;
    }

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
            ABP.Core.Domain.Entities.Commerce commerce = null;
            
            if (!string.IsNullOrEmpty(request.CommerceUserId))
            {
                commerce = await _commerceRepository.GetByUserIdAsync(request.CommerceUserId);
            }
            else
            {
                commerce = await _commerceRepository.GetByIdAsync(request.CommerceId);
            }

            if (commerce == null)
                throw new ApiException("El comercio no existe.");
                
            if (!commerce.IsActive)
                throw new ApiException("El comercio no está activo.");

            // Get CardTransactions for this commerce (HermesPay transactions)
            var allCardTransactions = await _cardTransactionRepository.GetAllByCommerceIdAsync(commerce.Id);
            
            // Get credit cards to resolve last four digits
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
                        ? card.CardNumber.Substring(card.CardNumber.Length - 4)
                        : "****";
                    
                    return new PaymentTransactionDto
                    {
                        Id = ct.Id.ToString(),
                        TransactionDate = ct.TransactionDate,
                        Amount = ct.Amount,
                        CardLastFourDigits = lastFour,
                        Status = ct.Status == ABP.Core.Domain.Common.Enums.TransactionStatus.Approved ? "APROBADO" : "RECHAZADO"
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
    }
}
