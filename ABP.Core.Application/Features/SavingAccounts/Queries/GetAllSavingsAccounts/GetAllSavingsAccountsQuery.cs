using MediatR;

namespace ABP.Core.Application.Features.SavingAccounts.Queries.GetAllSavingsAccounts
{
    public class GetAllSavingsAccountsQuery : IRequest<object>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Status { get; set; } = "activa";
        public string Type { get; set; } = "todas";
        public string? Identification { get; set; }
    }
}
