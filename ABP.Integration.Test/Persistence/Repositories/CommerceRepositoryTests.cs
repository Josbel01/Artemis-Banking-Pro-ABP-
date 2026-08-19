using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ABP.Integration.Tests.Persistence.Repositories
{
    public class CommerceRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public CommerceRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_CommerceRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetByRncAsync_Should_Return_Correct_Commerce()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var commerce = new Commerce { Id = 1, Name = "Test", Rnc = "123456789" };
            context.Commerces.Add(commerce);
            await context.SaveChangesAsync();

            var repo = new CommerceRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Commerce>>>().Object);

            // Act
            var result = await repo.GetByRncAsync("123456789");

            // Assert
            result.Should().NotBeNull();
            result!.Rnc.Should().Be("123456789");
        }

        [Fact]
        public async Task GetByUserIdAsync_Should_Return_Correct_Commerce()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Commerces.Add(new Commerce { Id = 1, Name = "Test", UserId = "U1" });
            await context.SaveChangesAsync();

            var repo = new CommerceRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Commerce>>>().Object);

            // Act
            var result = await repo.GetByUserIdAsync("U1");

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be("U1");
        }

        [Fact]
        public async Task GetByEmailAsync_Should_Return_Correct_Commerce()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Commerces.Add(new Commerce { Id = 1, Name = "Test", Email = "test@test.com" });
            await context.SaveChangesAsync();

            var repo = new CommerceRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Commerce>>>().Object);

            // Act
            var result = await repo.GetByEmailAsync("test@test.com");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@test.com");
        }

        [Fact]
        public async Task ExistsRncAsync_Should_Return_True_If_Exists()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Commerces.Add(new Commerce { Id = 1, Name = "Test", Rnc = "111" });
            await context.SaveChangesAsync();

            var repo = new CommerceRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Commerce>>>().Object);

            // Act
            var exists = await repo.ExistsRncAsync("111");

            // Assert
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsEmailAsync_Should_Return_True_If_Exists()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Commerces.Add(new Commerce { Id = 1, Name = "Test", Email = "a@a.com" });
            await context.SaveChangesAsync();

            var repo = new CommerceRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Commerce>>>().Object);

            // Act
            var exists = await repo.ExistsEmailAsync("a@a.com");

            // Assert
            exists.Should().BeTrue();
        }
    }
}
