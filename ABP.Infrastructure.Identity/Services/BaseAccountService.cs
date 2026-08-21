using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Dtos.User;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ABP.Infrastructure.Identity.Services
{
    public class BaseAccountService : IBaseAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger _baseAccountServiceLogger;

        public BaseAccountService(UserManager<AppUser> userManager, IEmailService emailService, ILogger loggerService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _baseAccountServiceLogger = loggerService;
        }

        public virtual async Task<UserResponseDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);

            UserResponseDto responseDto = new()
            {
                HasError = false,
                Errors = []
            };

            if(user == null)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add("No existe una cuenta registrada para este usuario.");
            }

            var result = await _userManager.ChangePasswordAsync(user!, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                responseDto.HasError = true;
                responseDto.Errors = result.Errors.Select(e => e.Description).ToList();
            }

            return responseDto;
        }

        public virtual async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            UserResponseDto responseDto = new() 
            { 
                HasError = false, 
                Errors = [] 
            };

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                responseDto.Message = "No existe una cuenta registrada para este usuario.";
                responseDto.HasError = true;
                
                return responseDto;
            }

            if (user.IsActive)
            {
                responseDto.HasError = true;
                responseDto.Message = "La cuenta ya está activada.";
                return responseDto;
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
                responseDto.Message = $"Cuenta confirmada para {user.Email}. Ya puedes iniciar sesión.";
                responseDto.HasError = false;
                return responseDto;
            }
            else
            {
                responseDto.Message = $"Ocurrió un error al confirmar el correo {user.Email}.";
                responseDto.HasError = true;
                return responseDto;
            }
        }

        public virtual async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto responserDto = new()
            {
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByIdAsync(id);

            if(user == null)
            {
                responserDto.HasError = true;
                responserDto.Errors.Add("No existe una cuenta registrada para este usuario.");
                return responserDto;
            }

            await _userManager.DeleteAsync(user);
            return responserDto;
        }

        public virtual async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false)
        {
            if (!string.IsNullOrWhiteSpace(saveDto.DNI))
                saveDto.DNI = saveDto.DNI.Replace("-", "");

            saveDto.FirstName = saveDto.FirstName?.Trim()!;
            saveDto.LastName = saveDto.LastName?.Trim()!;
            saveDto.UserName = saveDto.UserName?.Trim()!;
            saveDto.Email = saveDto.Email?.Trim()!;

            bool isNotCreated = !(isCreated ?? false);

            EditResponseDto responseDto = new()
            {
                Id = "",
                Email = "",
                FirstName = "",
                LastName = "",
                UserName = "",
                HasError = false,
                Errors = []
            };

            var normalizedUsername = saveDto.UserName?.ToUpper();
            var userWithSameUsername = await _userManager.Users.FirstOrDefaultAsync(w => w.NormalizedUserName == normalizedUsername && w.Id != saveDto.Id);

            if (userWithSameUsername != null)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add("Ya existe un usuario registrado con este nombre de usuario.");
                return responseDto;
            }

            if (!string.IsNullOrWhiteSpace(saveDto.DNI))
            {
                var userWithSameDni = await _userManager.Users.FirstOrDefaultAsync(w => w.Identification == saveDto.DNI && w.Id != saveDto.Id);
                if (userWithSameDni != null)
                {
                    responseDto.HasError = true;
                    responseDto.Errors.Add("Ya existe un usuario registrado con esta cédula.");
                    return responseDto;
                }
            }

            var user = await _userManager.FindByIdAsync(saveDto.Id ?? string.Empty);

            if(user == null)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add("No existe una cuenta registrada para este usuario.");
                return responseDto;
            }

            user.Name = saveDto.FirstName;
            user.LastName = saveDto.LastName;
            user.UserName = saveDto.UserName;
            user.Identification = saveDto.DNI;
            user.ProfileImage = string.IsNullOrWhiteSpace(saveDto.PhotoUrl) ? user.ProfileImage : saveDto.PhotoUrl;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            user.Email = saveDto.Email;
            user.PhoneNumber = saveDto.PhoneNumber ?? string.Empty;
            user.IsActive = saveDto.IsActive;

            if(!string.IsNullOrWhiteSpace(saveDto.Password) && isNotCreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if(resultChange != null && !resultChange.Succeeded)
                {
                    responseDto.HasError = true;
                    responseDto.Errors.AddRange(resultChange.Errors.Select(s => s.Description).ToList());
                    return responseDto;
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var roleList = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, roleList.ToList());
                await _userManager.AddToRoleAsync(user, saveDto.Role);

                if (!user.EmailConfirmed && isNotCreated)
                {
                    if(isApi != null && !isApi.Value)
                    {
                        string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            HtmlBody = $"Por favor confirme su cuenta visitando este enlace: {verificationUri}",
                            Subject = "Confirmar registro"
                        });
                    }
                    else
                    {
                        string? verificationToken = await GetVerificationEmailToken(user);
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            HtmlBody = $"Por favor confirme su cuenta usando este token: {verificationToken}",
                            Subject = "Confirmar registro"
                        });
                    }
                }

                var updateRoleList = await _userManager.GetRolesAsync(user);

                responseDto.Id = user.Id;
                responseDto.FirstName = user.Name;
                responseDto.LastName = user.LastName;
                responseDto.UserName = user.UserName ?? "";
                responseDto.Email = user.Email ?? "";
                responseDto.IsVerified = user.EmailConfirmed;
                responseDto.Roles = updateRoleList.ToList();

                return responseDto;
            }
            else
            {
                responseDto.HasError = true;
                responseDto.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return responseDto;
            }
        }

        public virtual async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        {
            UserResponseDto responseDto = new()
            {
                HasError = false,
                Errors = []
            };

            AppUser? user = null;

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                user = await _userManager.FindByNameAsync(request.UserName);
            }

            if(user == null && !string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _userManager.FindByEmailAsync(request.Email);
            }

            if(user == null)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add("No existe una cuenta registrada para este usuario.");

                return responseDto;
            }

            user.EmailConfirmed = false;
            user.IsActive = false;

            await _userManager.UpdateAsync(user);

            if(isApi != null && !isApi.Value)
            {
                var resetUri = await GetResetPasswordUri(user, request.Origin ?? "");
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Por favor restablezca su contraseña visitando este enlace: {resetUri}",
                    Subject = "Restablecer contraseña - Artemis Banking Pro"
                });
            }
            else
            {
                string? resetToken = await GetResetPasswordToken(user);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    HtmlBody = $"Por favor restablezca su contraseña usando este token: {resetToken}",
                    Subject = "Restablecer contraseña - Artemis Banking Pro"
                });
            }

            return responseDto;
        }

        public virtual async Task<List<UserDto>> GetAllUser(bool? isActive = true)
        {
            List<UserDto> listUsersDtos = [];

            var users = _userManager.Users;

            if(isActive != null)
            {
                users = users.Where(u => u.IsActive == isActive.Value);
            }

            var listUsers = await users.OrderByDescending(u => u.CreatedAt).ToListAsync();

            foreach(var item in listUsers)
            {
                var roleList = await _userManager.GetRolesAsync(item);

                listUsersDtos.Add(new UserDto()
                {
                    Id = item.Id,
                    Email = item.Email ?? "",
                    LastName = item.LastName,
                    FirstName = item.Name,
                    UserName = item.UserName ?? "",
                    PhoneNumber = item.PhoneNumber,
                    DNI = item.Identification,
                    PhotoUrl = item.ProfileImage,
                    Roles = roleList.ToList(),
                    IsActive = item.IsActive,
                    CreatedAt = item.CreatedAt
                });
            }
            return listUsersDtos;
        }

        public virtual async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                FirstName = user.Name,
                UserName = user.UserName ?? "",
                PhoneNumber = user.PhoneNumber,
                DNI = user.Identification,
                PhotoUrl = user.ProfileImage,
                Roles = roleList.ToList(),
                IsActive = user.IsActive
            };

            return userDto;
        }

        public virtual async Task<UserDto?> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return null;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                FirstName = user.Name,
                UserName = user.UserName ?? "",
                PhoneNumber = user.PhoneNumber,
                DNI = user.Identification,
                PhotoUrl = user.ProfileImage,
                Roles = roleList.ToList(),
                IsActive = user.IsActive
            };

            return userDto;
        }

        public virtual async Task<UserDto?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if(user == null)
            {
                return null;
            }

            var roleList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                FirstName = user.Name,
                UserName = user.UserName ?? "",
                PhoneNumber = user.PhoneNumber,
                DNI = user.Identification,
                PhotoUrl = user.ProfileImage,
                Roles = roleList.ToList(),
                IsActive = user.IsActive
            };

            return userDto;
        }

        public virtual async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string? origin, bool? isApi = false)
        {
            if (!string.IsNullOrWhiteSpace(saveDto.DNI))
                saveDto.DNI = saveDto.DNI.Replace("-", "");

            saveDto.UserName = saveDto.UserName?.Trim()!;
            saveDto.Email = saveDto.Email?.Trim()!;
            saveDto.FirstName = saveDto.FirstName?.Trim()!;
            saveDto.LastName = saveDto.LastName?.Trim()!;

            RegisterResponseDto responseDto = new()
            {
                Id = saveDto.Id ?? "",
                FirstName = "",
                LastName = "",
                UserName = "",
                Email = "",
                HasError = false,
                Errors = []
            };

            _baseAccountServiceLogger.LogInformation("Registering user {UserName} for web access", saveDto.UserName);
            var userWithSameUsername = await _userManager.FindByNameAsync(saveDto.UserName!);
            if (userWithSameUsername != null)
            {
                _baseAccountServiceLogger.LogWarning("Username {UserName} is already taken", saveDto.UserName);
                responseDto.HasError = true;
                responseDto.Errors.Add($"El nombre de usuario ya existe");

                return responseDto;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email!);
            if (userWithSameEmail != null)
            {
                _baseAccountServiceLogger.LogWarning("Email {Email} is already taken", saveDto.Email);
                responseDto.HasError = true;
                responseDto.Errors.Add($"Ya existe un usuario con este email.");

                return responseDto;
            }

            if (!string.IsNullOrWhiteSpace(saveDto.DNI))
            {
                var userWithSameDni = await _userManager.Users.FirstOrDefaultAsync(u => u.Identification == saveDto.DNI);
                if (userWithSameDni != null)
                {
                    responseDto.HasError = true;
                    responseDto.Errors.Add("Ya existe un usuario registrado con esta cédula.");
                    return responseDto;
                }
            }

            AppUser user = new AppUser()
            {
                Name = saveDto.FirstName ?? "",
                LastName = saveDto.LastName ?? "",
                UserName = saveDto.UserName ?? "",
                Identification = saveDto.DNI,
                Email = saveDto.Email,
                ProfileImage = saveDto.PhotoUrl ?? string.Empty,
                EmailConfirmed = false
            };

            // All users created from the system start inactive (only seeded users are active)

            _baseAccountServiceLogger.LogInformation("Creating user {UserName} with email {Email}", saveDto.UserName, saveDto.Email);
            var result = await _userManager.CreateAsync(user, saveDto.Password!);

            _baseAccountServiceLogger.LogInformation("User creation result for {UserName}: {Succeeded}", saveDto.UserName, result.Succeeded);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, saveDto.Role!);

                if (!user.EmailConfirmed)
                {
                    if(isApi != null && !isApi.Value)
                    {
                        string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            HtmlBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#1a56db,#3b82f6);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128231; Confirmaci&#243;n de Cuenta</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{user.Name}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Su cuenta ha sido registrada correctamente en <strong>Artemis Banking Pro</strong>. Para activar su usuario, haga clic en el siguiente bot&#243;n:</p>
<div style="text-align:center;margin:0 0 24px;">
<a href="{verificationUri}" style="display:inline-block;background:linear-gradient(135deg,#1a56db,#3b82f6);color:#fff;text-decoration:none;padding:14px 36px;border-radius:8px;font-weight:700;font-size:15px;">&#9989; Activar Mi Cuenta</a>
</div>
<div style="background:#f8fafc;border-radius:8px;padding:16px;margin-bottom:20px;">
<p style="color:#64748b;font-size:12px;margin:0 0 8px;">Si el bot&#243;n no funciona, copie y pegue este enlace en su navegador:</p>
<p style="color:#1a56db;font-size:11px;margin:0;word-break:break-all;">{verificationUri}</p>
</div>
<div style="background:#fef9c3;border-left:4px solid #ca8a04;padding:12px 16px;border-radius:0 6px 6px 0;">
<p style="color:#854d0e;font-size:12px;margin:0;">Si usted no realiz&#243; este registro, puede ignorar este mensaje.</p>
</div>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
""",
                            Subject = "Activación de Cuenta - Artemis Banking Pro"
                        });
                    }
                    else
                    {
                        string? verificationToken = await GetVerificationEmailToken(user);
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            HtmlBody = $"<p>Hola {user.Name},</p><p>Su cuenta ha sido registrada correctamente en Artemis Banking.</p>" +
                            $"<p>Por favor confirme su cuenta usando este token: <strong>{verificationToken}</strong></p><br/>" +
                            $"<p><small>Si usted no realizó este registro, puede ignorar este mensaje.</small></p>",
                            Subject = "Confirmacion de Cuenta"
                        });
                    }
                }

                var roleList = await _userManager.GetRolesAsync(user);

                responseDto.Id = user.Id;
                responseDto.FirstName = user.Name;
                responseDto.LastName = user.LastName;
                responseDto.Email = user.Email;
                responseDto.IsVerified = user.EmailConfirmed;
                responseDto.Roles = roleList.ToList();

                return responseDto;
            }
            else
            {
                responseDto.HasError = true;
                responseDto.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return responseDto;
            }
        }

        public virtual async Task<UserResponseDto> ResendActivationAsync(string userName, string? origin)
        {
            var responseDto = new UserResponseDto { Errors = [] };
            var user = await GetUserByUserName(userName.Trim());

            if (user != null && !user.IsActive)
            {
                var saveDto = new SaveUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DNI = user.DNI ?? "",
                    UserName = user.UserName,
                    Password = "",
                    ConfirmPassword = "",
                    Role = user.Roles?.FirstOrDefault() ?? "",
                    IsActive = user.IsActive
                };
                await EditUser(saveDto, origin, false, false);
            }
            return responseDto;
        }

        public virtual async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto responseDto = new()
            {
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByEmailAsync(request.Email);

            if(user == null)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add("No existe una cuenta registrada para este usuario.");
                return responseDto;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);

            if (!result.Succeeded)
            {
                responseDto.HasError = true;
                responseDto.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());

                return responseDto;
            }

            user.EmailConfirmed = true;
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            return responseDto;
        }

        #region Protected Methods

        protected async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Account/ConfirmEmail";
            string completeUrl;
            if (!string.IsNullOrEmpty(origin))
            {
                completeUrl = new Uri(string.Concat(origin.TrimEnd('/'), "/", route)).ToString();
            }
            else
            {
                completeUrl = "/" + route;
            }
            var verificationUri = QueryHelpers.AddQueryString(completeUrl, "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }

        protected async Task<string?> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Account/ResetPassword";
            string completeUrl;
            if (!string.IsNullOrEmpty(origin))
            {
                completeUrl = new Uri(string.Concat(origin.TrimEnd('/'), "/", route)).ToString();
            }
            else
            {
                completeUrl = "/" + route;
            }
            var queryParams = new Dictionary<string, string?>
            {
                { "token", token },
                { "email", user.Email }
            };
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), queryParams);

            return resetUri;
        }

        protected async Task<string?> GetVerificationEmailToken(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        protected async Task<string?> GetResetPasswordToken(AppUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }

        #endregion
    }
}
