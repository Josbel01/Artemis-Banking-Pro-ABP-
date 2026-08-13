using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABP.Infrastructure.Identity.Seeds
{
    public class DefaultCommerceUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                Name = "Pedro",
                LastName = "Commerce",
                Identification = "33333333333",
                Email = "Pedro@email.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "basicCommerce",
                IsActive = true
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if (entityUser == null)
                {
                    await userManager.CreateAsync(user, "Commerce_123*");
                    await userManager.AddToRoleAsync(user, UserRoles.Commerce.ToString());
                }
            }
        }
    }
}
