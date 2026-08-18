using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Commerces.Commands.UpdateCommerceStatus
{
    public class UpdateCommerceStatusCommandHandler : IRequestHandler<UpdateCommerceStatusCommand, bool>
    {
        private readonly ICommerceRepository _commerceRepository;
        private readonly IAccountServiceWebApi _userManager;

        public UpdateCommerceStatusCommandHandler(ICommerceRepository commerceRepository, IAccountServiceWebApi userManager)
        {
            _commerceRepository = commerceRepository;
            _userManager = userManager;
        }

        public async Task<bool> Handle(UpdateCommerceStatusCommand request, CancellationToken cancellationToken)
        {
            var commerce = await _commerceRepository.GetByIdAsync(request.Id);
            if (commerce == null) return false;

            commerce.IsActive = request.Status;
            await _commerceRepository.UpdateAsync(request.Id, commerce);

            if (!request.Status && !string.IsNullOrEmpty(commerce.UserId))
            {
                var user = await _userManager.GetUserById(commerce.UserId);
                if (user != null)
                {
                    var dto = new ABP.Core.Application.Dtos.User.SaveUserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DNI = user.DNI,
                        Email = user.Email,
                        UserName = user.UserName,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Roles?.FirstOrDefault() ?? "",
                        IsActive = false,
                        Password = "",
                        ConfirmPassword = ""
                    };
                    await _userManager.EditUser(dto, null, false, true);
                }
            }

            return true;
        }
    }
}
