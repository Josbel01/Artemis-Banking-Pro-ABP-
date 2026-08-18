using MediatR;

namespace ABP.Core.Application.Features.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQuery : IRequest<object>
    {
        public string Id { get; set; } = string.Empty;
    }
}
