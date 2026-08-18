using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ABP.Integration.Tests.Persistence.Repositories
{
    public class CreditCardRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public CreditCardRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_CreditCardRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetByCardNumberAsync_Should_Return_Card_With_Transactions()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var card = new CreditCard 
            { 
                Id = 1, 
                CardNumber = "1234567890123456", 
                ClientId = "C1",
                CardTransactions = new List<CardTransaction>
                {
                    new CardTransaction { Id = 1, Amount = 100, TransactionDate = DateTime.Now }
                }
            };
            context.CreditCards.Add(card);
            await context.SaveChangesAsync();

            var repo = new CreditCardRepository(context);

            // Act
            var result = await repo.GetByCardNumberAsync("1234567890123456");

            // Assert
            result.Should().NotBeNull();
            result!.CardNumber.Should().Be("1234567890123456");
            result.CardTransactions.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllByClientIdAsync_Should_Return_Cards_OrderedBy_CreatedAt_Desc()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.CreditCards.AddRange(
                new CreditCard { Id = 1, ClientId = "C1", CardNumber = "111", CreatedAt = new DateTime(2026, 1, 1) },
                new CreditCard { Id = 2, ClientId = "C1", CardNumber = "222", CreatedAt = new DateTime(2026, 1, 3) },
                new CreditCard { Id = 3, ClientId = "C1", CardNumber = "333", CreatedAt = new DateTime(2026, 1, 2) },
                new CreditCard { Id = 4, ClientId = "C2", CardNumber = "444", CreatedAt = new DateTime(2026, 1, 5) }
            );
            await context.SaveChangesAsync();

            var repo = new CreditCardRepository(context);

            // Act
            var result = await repo.GetAllByClientIdAsync("C1");

            // Assert
            result.Should().HaveCount(3);
            result[0].CardNumber.Should().Be("222"); // Latest created
            result[1].CardNumber.Should().Be("333");
            result[2].CardNumber.Should().Be("111"); // Oldest created
        }

        [Fact]
        public async Task ExistsCardNumberAsync_Should_Check_Cards()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.CreditCards.Add(new CreditCard { Id = 1, CardNumber = "111", ClientId = "C" });
            await context.SaveChangesAsync();

            var repo = new CreditCardRepository(context);

            // Act
            var exists1 = await repo.ExistsCardNumberAsync("111");
            var exists2 = await repo.ExistsCardNumberAsync("222");

            // Assert
            exists1.Should().BeTrue();
            exists2.Should().BeFalse();
        }
    }
}
