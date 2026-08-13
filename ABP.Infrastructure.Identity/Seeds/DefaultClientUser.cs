using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ABP.Infrastructure.Identity.Seeds
{
    public class DefaultClientUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                Name = "Josbel",
                LastName = "Alvarez",
                Identification = "22222222222",
                Email = "Josbel@email.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "basicClient",
                IsActive = true
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if (entityUser == null)
                {
                    await userManager.CreateAsync(user, "Client_123*");
                    await userManager.AddToRoleAsync(user, UserRoles.Client.ToString());
                }
            }
        }
    }
}
