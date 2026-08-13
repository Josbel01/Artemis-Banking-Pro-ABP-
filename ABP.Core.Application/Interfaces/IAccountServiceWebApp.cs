using ABP.Core.Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface IAccountServiceWebApp : IBaseAccountService
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task SignOutAsync();
    }
}
