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
    public class CardTransactionRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public CardTransactionRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_CardTransactionRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetAllByCreditCardIdAsync_Should_Return_Transactions_OrderedBy_Date_Desc()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.CardTransactions.AddRange(
                new CardTransaction { Id = 1, CreditCardId = 1, Amount = 100, TransactionDate = new DateTime(2026, 1, 1) },
                new CardTransaction { Id = 2, CreditCardId = 1, Amount = 200, TransactionDate = new DateTime(2026, 1, 3) },
                new CardTransaction { Id = 3, CreditCardId = 1, Amount = 300, TransactionDate = new DateTime(2026, 1, 2) },
                new CardTransaction { Id = 4, CreditCardId = 2, Amount = 400, TransactionDate = new DateTime(2026, 1, 5) }
            );
            await context.SaveChangesAsync();

            var repo = new CardTransactionRepository(context);

            // Act
            var result = await repo.GetAllByCreditCardIdAsync(1);

            // Assert
            result.Should().HaveCount(3);
            result[0].Amount.Should().Be(200); // Latest date
            result[1].Amount.Should().Be(300);
            result[2].Amount.Should().Be(100); // Oldest date
        }

        [Fact]
        public async Task GetAllByCommerceIdAsync_Should_Return_Transactions_OrderedBy_Date_Desc()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.CardTransactions.AddRange(
                new CardTransaction { Id = 1, CommerceId = 1, Amount = 100, TransactionDate = new DateTime(2026, 1, 1) },
                new CardTransaction { Id = 2, CommerceId = 1, Amount = 200, TransactionDate = new DateTime(2026, 1, 3) },
                new CardTransaction { Id = 3, CommerceId = 1, Amount = 300, TransactionDate = new DateTime(2026, 1, 2) },
                new CardTransaction { Id = 4, CommerceId = 2, Amount = 400, TransactionDate = new DateTime(2026, 1, 5) }
            );
            await context.SaveChangesAsync();

            var repo = new CardTransactionRepository(context);

            // Act
            var result = await repo.GetAllByCommerceIdAsync(1);

            // Assert
            result.Should().HaveCount(3);
            result[0].Amount.Should().Be(200); // Latest date
            result[1].Amount.Should().Be(300);
            result[2].Amount.Should().Be(100); // Oldest date
        }
    }
}
