using ABP.Core.Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface IAccountServiceWebApi : IBaseAccountService
    {
        Task<LoginResponseApiDto> Login(LoginDto loginDto);
        Task<bool> UpdateUserStatusAsync(string id, bool status);
    }
}
