using ABP.Infrastructure.Identity.Contexts;
using ABP.Infrastructure.Identity.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ABP.Integration.Test.Persistence.Identity
{
    public class IdentityIntegrationTests
    {
        private ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName: $"IdentityTestDb_{Guid.NewGuid()}");
            });

            services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

            services.AddLogging();
            services.AddDataProtection();

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task CreateUserAsync_Should_Create_Inactive_User()
        {
            // Arrange
            using var serviceProvider = BuildServiceProvider();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = new AppUser
            {
                Name = "Test",
                LastName = "User",
                UserName = "testuser",
                Email = "testuser@test.com",
                Identification = "00100000001",
                IsActive = false // Default as per requirements
            };

            // Act
            var result = await userManager.CreateAsync(user, "Password123!");

            // Assert
            result.Succeeded.Should().BeTrue();
            var createdUser = await userManager.FindByNameAsync("testuser");
            createdUser.Should().NotBeNull();
            createdUser!.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task AddUserToRoleAsync_Should_Assign_Role()
        {
            // Arrange
            using var serviceProvider = BuildServiceProvider();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var user = new AppUser
            {
                Name = "Role",
                LastName = "Tester",
                UserName = "roletester",
                Email = "role@test.com",
                Identification = "00100000002"
            };
            await userManager.CreateAsync(user, "Password123!");
            await roleManager.CreateAsync(new IdentityRole("Admin"));

            // Act
            var result = await userManager.AddToRoleAsync(user, "Admin");

            // Assert
            result.Succeeded.Should().BeTrue();
            var isInRole = await userManager.IsInRoleAsync(user, "Admin");
            isInRole.Should().BeTrue();
        }

        [Fact]
        public async Task GenerateEmailConfirmationTokenAsync_Should_Generate_Token()
        {
            // Arrange
            using var serviceProvider = BuildServiceProvider();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = new AppUser
            {
                Name = "Token",
                LastName = "Tester",
                UserName = "tokentester",
                Email = "token@test.com",
                Identification = "00100000003"
            };
            await userManager.CreateAsync(user, "Password123!");

            // Act
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            var confirmResult = await userManager.ConfirmEmailAsync(user, token);
            confirmResult.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task CheckPasswordAsync_Should_Return_True_For_Valid_Password()
        {
            // Arrange
            using var serviceProvider = BuildServiceProvider();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = new AppUser
            {
                Name = "Pass",
                LastName = "Tester",
                UserName = "passtester",
                Email = "pass@test.com",
                Identification = "00100000004"
            };
            await userManager.CreateAsync(user, "ComplexPass123!");

            // Act
            var isValid = await userManager.CheckPasswordAsync(user, "ComplexPass123!");
            var isInvalid = await userManager.CheckPasswordAsync(user, "WrongPass123!");

            // Assert
            isValid.Should().BeTrue();
            isInvalid.Should().BeFalse();
        }
    }
}
