using ABP.Core.Application.Dtos.CreditCards;
using ABP.Core.Application.Dtos.Loans;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface ILoanService : IGenericService<LoanDto>
    {
        Task<List<LoanDto>> GetAllByClientIdAsync(string clientId);
        Task<LoanDto?> GetByLoanNumberAsync(string loanNumber);
        decimal CalculateClientDebt(string clientId, List<LoanDto> loans, List<CreditCardDto> creditCards);
        Task<(bool hasRisk, string message, decimal avgDebt, decimal currentDebt, decimal projectedDebt)> EvaluateRiskAsync(string clientId, decimal principal, decimal rate, int term);
        Task<LoanDto?> ProcessLoanCreationAsync(string clientId, decimal principal, decimal rate, int term, string assignedByUserId);
    }
}
