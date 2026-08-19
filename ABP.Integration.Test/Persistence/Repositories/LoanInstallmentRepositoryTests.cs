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
    public class LoanInstallmentRepositoryTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public LoanInstallmentRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_LoanInstallmentRepo_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task GetAllByLoanIdAsync_Should_Return_Installments_Ordered_By_InstallmentNumber()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            context.LoanInstallments.AddRange(
                new LoanInstallment { Id = 1, LoanId = 1, InstallmentNumber = 3, InstallmentAmount = 100 },
                new LoanInstallment { Id = 2, LoanId = 1, InstallmentNumber = 1, InstallmentAmount = 100 },
                new LoanInstallment { Id = 3, LoanId = 1, InstallmentNumber = 2, InstallmentAmount = 100 },
                new LoanInstallment { Id = 4, LoanId = 2, InstallmentNumber = 1, InstallmentAmount = 100 }
            );
            await context.SaveChangesAsync();

            var repo = new LoanInstallmentRepository(context, new Moq.Mock<Microsoft.Extensions.Logging.ILogger<GenericRepository<LoanInstallment>>>().Object);

            // Act
            var result = await repo.GetAllByLoanIdAsync(1);

            // Assert
            result.Should().HaveCount(3);
            result[0].InstallmentNumber.Should().Be(1);
            result[1].InstallmentNumber.Should().Be(2);
            result[2].InstallmentNumber.Should().Be(3);
        }
    }
}
