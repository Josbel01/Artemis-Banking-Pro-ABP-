using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ABP.Infrastructure.Identity.Seeds
{
    public static class DefaultClientUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, DbContext? dbContext = null)
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

                    // Create main saving account for the new client
                    await CreateMainAccountIfNeeded(dbContext, user.Id);
                }
            }
            else
            {
                // User already exists - check if they need a main account
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    await CreateMainAccountIfNeeded(dbContext, existingUser.Id);
                }
            }
        }

        private static async Task CreateMainAccountIfNeeded(DbContext? dbContext, string clientId)
        {
            if (dbContext == null) return;

            var existingAccount = await dbContext.Set<SavingAccount>()
                .FirstOrDefaultAsync(a => a.ClientId == clientId && a.AccountType == SavingAccountType.Main);

            if (existingAccount == null)
            {
                var rnd = new Random();
                string accountNumber = rnd.Next(100000000, 999999999).ToString();
                var mainAccount = new SavingAccount
                {
                    ClientId = clientId,
                    AccountNumber = accountNumber,
                    Balance = 0,
                    AccountType = SavingAccountType.Main,
                    Status = SavingAccountStatus.Active
                };
                dbContext.Set<SavingAccount>().Add(mainAccount);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
