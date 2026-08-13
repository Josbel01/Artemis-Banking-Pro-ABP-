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
    public class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            AppUser user = new()
            {
                Name = "Gerardine",
                LastName = "Roa", Identification = "00000000000",
                Email = "Gerardine@email.com",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                UserName = "basicAdmin",
                IsActive = true
            };

            if (await userManager.Users.AllAsync(u => u.Id != user.Id))
            {
                var entityUser = await userManager.FindByEmailAsync(user.Email);
                if (entityUser == null)
                {
                    await userManager.CreateAsync(user, "Admin_123*");
                    await userManager.AddToRoleAsync(user, UserRoles.Admin.ToString());
                }
            }
        }
    }
}
