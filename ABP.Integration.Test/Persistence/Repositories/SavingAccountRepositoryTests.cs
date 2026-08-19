using ABP.Core.Domain.Common.Enums;
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
    public class SavingAccountRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public SavingAccountRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_SavingRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetByAccountNumberAsync_Should_Return_Account_With_Transactions()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var account = new SavingAccount 
            { 
                Id = 1, 
                AccountNumber = "123456789", 
                ClientId = "C1",
                Transactions = new List<Transaction>
                {
                    new Transaction { Id = 1, Type = TransactionType.Credit, Amount = 100, TransactionDate = DateTime.Now }
                }
            };
            context.SavingAccounts.Add(account);
            await context.SaveChangesAsync();

            var repo = new SavingAccountRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<SavingAccount>>>().Object);

            // Act
            var result = await repo.GetByAccountNumberAsync("123456789");

            // Assert
            result.Should().NotBeNull();
            result!.AccountNumber.Should().Be("123456789");
            result.Transactions.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetAllByClientIdAsync_Should_Return_Only_Active_Accounts()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.SavingAccounts.AddRange(
                new SavingAccount { Id = 1, ClientId = "C1", Status = SavingAccountStatus.Active, AccountType = SavingAccountType.Main, Balance = 1000 },
                new SavingAccount { Id = 2, ClientId = "C1", Status = SavingAccountStatus.Active, AccountType = SavingAccountType.Secondary, Balance = 2000 },
                new SavingAccount { Id = 3, ClientId = "C1", Status = SavingAccountStatus.Cancelled, AccountType = SavingAccountType.Secondary, Balance = 0 }
            );
            await context.SaveChangesAsync();

            var repo = new SavingAccountRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<SavingAccount>>>().Object);

            // Act
            var result = await repo.GetAllByClientIdAsync("C1");

            // Assert
            result.Should().HaveCount(2);
            result[0].AccountType.Should().Be(SavingAccountType.Main);
        }

        [Fact]
        public async Task GetPrincipalAccountByClientIdAsync_Should_Return_Main_Active_Account()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.SavingAccounts.AddRange(
                new SavingAccount { Id = 1, ClientId = "C2", Status = SavingAccountStatus.Active, AccountType = SavingAccountType.Secondary },
                new SavingAccount { Id = 2, ClientId = "C2", Status = SavingAccountStatus.Active, AccountType = SavingAccountType.Main }
            );
            await context.SaveChangesAsync();

            var repo = new SavingAccountRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<SavingAccount>>>().Object);

            // Act
            var result = await repo.GetPrincipalAccountByClientIdAsync("C2");

            // Assert
            result.Should().NotBeNull();
            result!.AccountType.Should().Be(SavingAccountType.Main);
        }

        [Fact]
        public async Task ExistsAccountNumberAsync_Should_Check_SavingAccounts_And_Loans()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.SavingAccounts.Add(new SavingAccount { Id = 1, AccountNumber = "111", ClientId = "C" });
            context.Loans.Add(new Loan { Id = 1, LoanNumber = "222", ClientId = "C" });
            await context.SaveChangesAsync();

            var repo = new SavingAccountRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<SavingAccount>>>().Object);

            // Act
            var exists1 = await repo.ExistsAccountNumberAsync("111");
            var exists2 = await repo.ExistsAccountNumberAsync("222");
            var exists3 = await repo.ExistsAccountNumberAsync("333");

            // Assert
            exists1.Should().BeTrue();
            exists2.Should().BeTrue();
            exists3.Should().BeFalse();
        }
    }
}
