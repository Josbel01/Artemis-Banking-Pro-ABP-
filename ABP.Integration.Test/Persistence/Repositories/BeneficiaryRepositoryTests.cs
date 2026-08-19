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
    public class BeneficiaryRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public BeneficiaryRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_BeneficiaryRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetAllByClientIdAsync_Should_Return_Beneficiaries_OrderedBy_CreatedAt_Desc()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Beneficiaries.AddRange(
                new Beneficiary { Id = 1, ClientId = "C1", BeneficiaryAccountNumber = "111", CreatedAt = new DateTime(2026, 1, 1) },
                new Beneficiary { Id = 2, ClientId = "C1", BeneficiaryAccountNumber = "222", CreatedAt = new DateTime(2026, 1, 3) },
                new Beneficiary { Id = 3, ClientId = "C1", BeneficiaryAccountNumber = "333", CreatedAt = new DateTime(2026, 1, 2) },
                new Beneficiary { Id = 4, ClientId = "C2", BeneficiaryAccountNumber = "444", CreatedAt = new DateTime(2026, 1, 5) }
            );
            await context.SaveChangesAsync();

            var repo = new BeneficiaryRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Beneficiary>>>().Object);

            // Act
            var result = await repo.GetAllByClientIdAsync("C1");

            // Assert
            result.Should().HaveCount(3);
            result[0].BeneficiaryAccountNumber.Should().Be("222"); // Latest created
            result[1].BeneficiaryAccountNumber.Should().Be("333");
            result[2].BeneficiaryAccountNumber.Should().Be("111"); // Oldest created
        }

        [Fact]
        public async Task GetByAccountAndClientIdAsync_Should_Return_Correct_Beneficiary()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Beneficiaries.AddRange(
                new Beneficiary { Id = 1, ClientId = "C1", BeneficiaryAccountNumber = "111" },
                new Beneficiary { Id = 2, ClientId = "C2", BeneficiaryAccountNumber = "222" }
            );
            await context.SaveChangesAsync();

            var repo = new BeneficiaryRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Beneficiary>>>().Object);

            // Act
            var result = await repo.GetByAccountAndClientIdAsync("222", "C2");
            var resultNotFound = await repo.GetByAccountAndClientIdAsync("111", "C2");

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(2);

            resultNotFound.Should().BeNull();
        }
    }
}
