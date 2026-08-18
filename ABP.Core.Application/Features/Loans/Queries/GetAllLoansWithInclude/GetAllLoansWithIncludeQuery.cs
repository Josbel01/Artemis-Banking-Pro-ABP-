using MediatR;

namespace ABP.Core.Application.Features.Loans.Queries.GetAllLoansWithInclude
{
    public class GetAllLoansWithIncludeQuery : IRequest<object>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
