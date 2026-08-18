using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;



namespace ABP.Core.Application.Features.Commerces.Queries.GetCommerceById
{
    public class GetCommerceByIdQueryHandler : IRequestHandler<GetCommerceByIdQuery, object?>
    {
        private readonly ICommerceRepository _commerceRepository;
        private readonly IAccountServiceWebApi _userManager;

        public GetCommerceByIdQueryHandler(ICommerceRepository commerceRepository, IAccountServiceWebApi userManager)
        {
            _commerceRepository = commerceRepository;
            _userManager = userManager;
        }

        public async Task<object?> Handle(GetCommerceByIdQuery request, CancellationToken cancellationToken)
        {
            var commerce = await _commerceRepository.GetByIdAsync(request.Id);
            if (commerce == null) return null;

            object? associatedUser = null;

            if (!string.IsNullOrEmpty(commerce.UserId))
            {
                var user = await _userManager.GetUserById(commerce.UserId);
                if (user != null)
                {
                    associatedUser = new {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        isActive = user.IsActive
                    };
                }
            }

            return new
            {
                id = commerce.Id,
                name = commerce.Name,
                description = commerce.Description,
                email = commerce.Email,
                phoneNumber = commerce.PhoneNumber,
                rnc = commerce.Rnc,
                isActive = commerce.IsActive,
                createdAt = System.DateTime.Now,
                associatedUser = associatedUser
            };
        }
    }
}

