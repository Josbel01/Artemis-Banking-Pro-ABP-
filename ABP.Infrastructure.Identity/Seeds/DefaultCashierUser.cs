using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ABP.Infrastructure.Identity.Seeds
{
    public class DefaultCashierUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                Name = "Victor",
                LastName = "Nunez",
                Email = "Victor@email.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "basicCashier",
                IsActive = true
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if (entityUser != null)
                {
                    await userManager.CreateAsync(user, "Cashier_123*");
                    await userManager.AddToRoleAsync(user, UserRoles.Cashier.ToString());
                }
            }
        }
    }
}
