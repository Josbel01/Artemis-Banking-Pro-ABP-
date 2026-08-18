using MediatR;

namespace ABP.Core.Application.Features.Commerces.Queries.GetCommerceById
{
    public class GetCommerceByIdQuery : IRequest<object>
    {
        public int Id { get; set; }
    }
}
