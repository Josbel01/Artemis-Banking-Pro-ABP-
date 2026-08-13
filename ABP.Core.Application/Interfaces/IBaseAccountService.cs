using ABP.Core.Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Core.Application.Interfaces
{
    public interface IBaseAccountService
    {
        Task<UserResponseDto> ConfirmAccountAsync(string userId, string token);
        Task<UserResponseDto> DeleteAsync(string id);
        Task<EditResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false);
        Task<List<UserDto>> GetAllUser(bool? isActive = true);
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserByUserName(string userName);
        Task<UserDto?> GetUserById(string id);
        Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string? origin, bool? isApi = false);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<UserResponseDto> ResendActivationAsync(string userName, string? origin);
        Task<UserResponseDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
