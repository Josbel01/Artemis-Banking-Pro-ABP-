using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Queries.GetAllCommerces
{
    public class GetAllCommercesQueryHandler : IRequestHandler<GetAllCommercesQuery, object>
    {
        private readonly ICommerceRepository _commerceRepository;

        public GetAllCommercesQueryHandler(ICommerceRepository commerceRepository)
        {
            _commerceRepository = commerceRepository;
        }

        public async Task<object> Handle(GetAllCommercesQuery request, CancellationToken cancellationToken)
        {
            var commerces = await _commerceRepository.GetAllListAsync();

            // Parse status filter: "activo"/"true", "inactivo"/"false", "todos"
            if (!string.IsNullOrEmpty(request.Status))
            {
                var statusLower = request.Status.ToLower().Trim();
                if (statusLower == "activo" || statusLower == "true" || statusLower == "1")
                {
                    commerces = commerces.Where(c => c.IsActive).ToList();
                }
                else if (statusLower == "inactivo" || statusLower == "false" || statusLower == "0")
                {
                    commerces = commerces.Where(c => !c.IsActive).ToList();
                }
                // "todos" = no filter, show all
            }
            else
            {
                // Default: show active only
                commerces = commerces.Where(c => c.IsActive).ToList();
            }

            commerces = commerces.OrderByDescending(c => c.Id).ToList();

            var paged = commerces.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new
            {
                page = request.Page,
                pageSize = request.PageSize,
                totalRecords = commerces.Count,
                totalPages = commerces.Count == 0 ? 1 : (int)Math.Ceiling(commerces.Count / (double)request.PageSize),
                data = paged.Select(c => new {
                    id = c.Id,
                    name = c.Name,
                    description = c.Description,
                    email = c.Email,
                    phoneNumber = c.PhoneNumber,
                    rnc = c.Rnc,
                    isActive = c.IsActive,
                    hasAssociatedUser = !string.IsNullOrEmpty(c.UserId),
                    createdAt = DateTime.Now // Replace with actual created at if added to entity
                })
            };
        }
    }
}

