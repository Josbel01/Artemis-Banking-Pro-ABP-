using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ABP.Infrastructure.Identity.Seeds
{
    public static class DefaultCommerceUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, DbContext? dbContext = null)
        {
            // 1. Create the AppUser with role Commerce
            AppUser user = new()
            {
                Name = "Tienda",
                LastName = "Demo",
                Identification = "33333333333",
                Email = "commerce@artemis.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "basicCommerce",
                IsActive = true
            };

            string userId = string.Empty;

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if (entityUser == null)
                {
                    var result = await userManager.CreateAsync(user, "Commerce_123*");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, UserRoles.Commerce.ToString());
                        userId = user.Id;
                    }
                }
                else
                {
                    userId = entityUser.Id;
                }
            }
            else
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    userId = existingUser.Id;
                }
            }

            // 2. Create the Commerce entity linked to this user
            if (!string.IsNullOrEmpty(userId) && dbContext != null)
            {
                var existingCommerce = await dbContext.Set<Commerce>()
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (existingCommerce == null)
                {
                    var commerce = new Commerce
                    {
                        Name = "Tienda Demo",
                        Description = "Comercio de prueba para pagos Hermes Pay",
                        Email = "commerce@artemis.com",
                        PhoneNumber = "8095551234",
                        Rnc = "101999999",
                        UserId = userId,
                        IsActive = true
                    };
                    dbContext.Set<Commerce>().Add(commerce);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
