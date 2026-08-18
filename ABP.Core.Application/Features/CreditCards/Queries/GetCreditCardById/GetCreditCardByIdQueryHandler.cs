using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ABP.Core.Application.Features.CreditCards.Queries.GetCreditCardById
{
    public class GetCreditCardByIdQueryHandler : IRequestHandler<GetCreditCardByIdQuery, object?>
    {
        private readonly ICreditCardService _creditCardService;
        private readonly ITransactionService _transactionService;

        public GetCreditCardByIdQueryHandler(ICreditCardService creditCardService, ITransactionService transactionService)
        {
            _creditCardService = creditCardService;
            _transactionService = transactionService;
        }

        public async Task<object?> Handle(GetCreditCardByIdQuery request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return null;

            var card = await _creditCardService.GetByIdAsync(id);
            if (card == null) return null;

            var transactions = await _transactionService.GetAllAsync();
            var cardTransactions = transactions.Where(t => t.SavingAccountId == id).OrderByDescending(t => t.TransactionDate).ToList();

            return new
            {
                id = card.Id.ToString(),
                maskedCardNumber = $"************{card.CardNumber.Substring(card.CardNumber.Length - 4)}",
                lastFourDigits = card.CardNumber.Substring(card.CardNumber.Length - 4),
                clientId = card.ClientId,
                clientFullName = "", 
                creditLimit = card.CreditLimit,
                availableCredit = (card.CreditLimit - card.CurrentDebt),
                currentDebt = card.CurrentDebt,
                expirationDate = card.ExpirationDate,
                status = card.Status,
                consumptions = cardTransactions.Select(t => new {
                    id = t.Id.ToString(),
                    date = t.TransactionDate,
                    amount = t.Amount,
                    transactionType = t.Type == ABP.Core.Domain.Common.Enums.TransactionType.Credit ? "CRÉDITO" : "DÉBITO",
                    status = "APROBADO" // Simplified
                })
            };
        }
    }



}
