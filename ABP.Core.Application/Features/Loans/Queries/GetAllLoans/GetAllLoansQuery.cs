using MediatR;

namespace ABP.Core.Application.Features.Loans.Queries.GetAllLoans
{
    public class GetAllLoansQuery : IRequest<object>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; }
        public string? Identification { get; set; }
    }
}
