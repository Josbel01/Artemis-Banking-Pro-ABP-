using ABP.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;


namespace ABP.Infrastructure.Identity.Seeds
{
    public static class DefaultUserRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { UserRoles.Admin, UserRoles.Cashier, UserRoles.Client, UserRoles.Commerce };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role.ToString()))
                {
                    await roleManager.CreateAsync(new IdentityRole(role.ToString()));
                }
            }
        }
    }
}
