using MediatR;

namespace ABP.Core.Application.Features.Commerces.Queries.GetAllCommerces
{
    public class GetAllCommercesQuery : IRequest<object>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; } = "activo";
    }
}
