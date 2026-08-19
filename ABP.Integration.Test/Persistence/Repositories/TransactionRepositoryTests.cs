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
    public class TransactionRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public TransactionRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_TransactionRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetAllBySavingAccountIdAsync_Should_Return_Transactions_OrderedBy_Date_Desc()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Transactions.AddRange(
                new Transaction { Id = 1, SavingAccountId = 1, Amount = 100, TransactionDate = new DateTime(2026, 1, 1) },
                new Transaction { Id = 2, SavingAccountId = 1, Amount = 200, TransactionDate = new DateTime(2026, 1, 3) },
                new Transaction { Id = 3, SavingAccountId = 1, Amount = 300, TransactionDate = new DateTime(2026, 1, 2) },
                new Transaction { Id = 4, SavingAccountId = 2, Amount = 400, TransactionDate = new DateTime(2026, 1, 5) }
            );
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Transaction>>>().Object);

            // Act
            var result = await repo.GetAllBySavingAccountIdAsync(1);

            // Assert
            result.Should().HaveCount(3);
            result[0].Amount.Should().Be(200); // Latest date
            result[1].Amount.Should().Be(300);
            result[2].Amount.Should().Be(100); // Oldest date
        }
    }
}
