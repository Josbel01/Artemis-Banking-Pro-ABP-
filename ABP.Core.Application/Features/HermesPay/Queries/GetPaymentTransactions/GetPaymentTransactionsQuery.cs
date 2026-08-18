using ABP.Core.Application.Exceptions;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly IMapper _mapper;

        public GetPaymentTransactionsQueryHandler(ICommerceRepository commerceRepository, ISavingAccountRepository savingAccountRepository, IMapper mapper)
        {
            _commerceRepository = commerceRepository;
            _savingAccountRepository = savingAccountRepository;
            _mapper = mapper;
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

            var principalAccount = await _savingAccountRepository.GetPrincipalAccountByClientIdAsync(commerce.UserId);
            
            var accountsWithTransactions = await _savingAccountRepository.GetAllListWithInclude(new List<string> { "Transactions" });
            var accountWithTransactions = accountsWithTransactions
                .FirstOrDefault(a => a.ClientId == commerce.UserId && a.AccountType == ABP.Core.Domain.Common.Enums.SavingAccountType.Main);

            if (accountWithTransactions == null)
            {
                return new PaymentTransactionResponse
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = 0,
                    TotalPages = 0,
                    CommerceId = commerce.Id,
                    CommerceName = commerce.Name,
                    Data = new List<PaymentTransactionDto>()
                };
            }

            var query = accountWithTransactions.Transactions
                .Where(t => t.Type == ABP.Core.Domain.Common.Enums.TransactionType.Credit && !string.IsNullOrEmpty(t.Origin))
                .OrderByDescending(t => t.TransactionDate)
                .ToList();

            int totalRecords = query.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

            var pagedData = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsQueryable()
                .ProjectTo<PaymentTransactionDto>(_mapper.ConfigurationProvider)
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
