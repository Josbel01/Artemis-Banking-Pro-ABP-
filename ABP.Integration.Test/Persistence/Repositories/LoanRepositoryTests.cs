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
    public class LoanRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public LoanRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_LoanRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetByLoanNumberAsync_Should_Return_Loan_With_Installments()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            var loan = new Loan 
            { 
                Id = 1, 
                LoanNumber = "123456789", 
                ClientId = "C1",
                LoanInstallments = new List<LoanInstallment>
                {
                    new LoanInstallment { Id = 1, InstallmentNumber = 2, InstallmentAmount = 100 },
                    new LoanInstallment { Id = 2, InstallmentNumber = 1, InstallmentAmount = 100 }
                }
            };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();

            var repo = new LoanRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Loan>>>().Object);

            // Act
            var result = await repo.GetByLoanNumberAsync("123456789");

            // Assert
            result.Should().NotBeNull();
            result!.LoanNumber.Should().Be("123456789");
            result.LoanInstallments.Should().HaveCount(2);
            result.LoanInstallments.Should().Contain(x => x.InstallmentNumber == 1);
            result.LoanInstallments.Should().Contain(x => x.InstallmentNumber == 2);
        }

        [Fact]
        public async Task GetAllByClientIdAsync_Should_Return_Loans_OrderedBy_CreatedAt_Desc()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.Loans.AddRange(
                new Loan { Id = 1, ClientId = "C1", LoanNumber = "111", CreatedAt = new DateTime(2026, 1, 1) },
                new Loan { Id = 2, ClientId = "C1", LoanNumber = "222", CreatedAt = new DateTime(2026, 1, 3) },
                new Loan { Id = 3, ClientId = "C1", LoanNumber = "333", CreatedAt = new DateTime(2026, 1, 2) },
                new Loan { Id = 4, ClientId = "C2", LoanNumber = "444", CreatedAt = new DateTime(2026, 1, 5) }
            );
            await context.SaveChangesAsync();

            var repo = new LoanRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Loan>>>().Object);

            // Act
            var result = await repo.GetAllByClientIdAsync("C1");

            // Assert
            result.Should().HaveCount(3);
            result[0].LoanNumber.Should().Be("222"); // Latest created
            result[1].LoanNumber.Should().Be("333");
            result[2].LoanNumber.Should().Be("111"); // Oldest created
        }

        [Fact]
        public async Task ExistsLoanNumberAsync_Should_Check_Loans_And_SavingAccounts()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.SavingAccounts.Add(new SavingAccount { Id = 1, AccountNumber = "111", ClientId = "C" });
            context.Loans.Add(new Loan { Id = 1, LoanNumber = "222", ClientId = "C" });
            await context.SaveChangesAsync();

            var repo = new LoanRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<Loan>>>().Object);

            // Act
            var exists1 = await repo.ExistsLoanNumberAsync("111");
            var exists2 = await repo.ExistsLoanNumberAsync("222");
            var exists3 = await repo.ExistsLoanNumberAsync("333");

            // Assert
            exists1.Should().BeTrue();
            exists2.Should().BeTrue();
            exists3.Should().BeFalse();
        }
    }
}
