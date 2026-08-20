using MediatR;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.Commerces.Commands.CreateCommerceUser
{
    public class CreateCommerceUserCommandHandler : IRequestHandler<CreateCommerceUserCommand, RegisterResponseDto>
    {
        private readonly IAccountServiceWebApi _accountService;
        private readonly ICommerceRepository _commerceRepository;

        public CreateCommerceUserCommandHandler(IAccountServiceWebApi accountService, ICommerceRepository commerceRepository)
        {
            _accountService = accountService;
            _commerceRepository = commerceRepository;
        }

        public async Task<RegisterResponseDto> Handle(CreateCommerceUserCommand request, CancellationToken cancellationToken)
        {
            var commerce = await _commerceRepository.GetByIdAsync(request.CommerceId);
            if (commerce == null)
            {
                return new RegisterResponseDto { HasError = true, Errors = new List<string> { "El comercio indicado no existe." }, Id = "", FirstName = "", LastName = "", Email = "", UserName = "" };
            }

            if (!string.IsNullOrEmpty(commerce.UserId))
            {
                return new RegisterResponseDto { HasError = true, Errors = new List<string> { "El comercio ya tiene un usuario asociado." }, Id = "", FirstName = "", LastName = "", Email = "", UserName = "" };
            }

            request.UserDto.Role = UserRoles.Commerce.ToString();

            var response = await _accountService.RegisterUser(request.UserDto, request.Origin, true);
            if (response.HasError) return response;

            commerce.UserId = response.Id;
            await _commerceRepository.UpdateAsync(commerce.Id, commerce);

            return response;
        }
    }
}
