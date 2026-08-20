using FluentAssertions;
using ABP.Core.Application.Features.Beneficiaries.Commands.CreateBeneficiary;
using ABP.Core.Domain.Entities;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.User;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ABP.Unit.Tests.Features.Beneficiaries.Commands.CreateBeneficiary
{
    public class CreateBeneficiaryCommandHandlerTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public CreateBeneficiaryCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_CreateBeneficiary_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Create_Beneficiary_When_Valid()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            
            var mockAccountService = new Mock<IBaseAccountService>();
            mockAccountService.Setup(x => x.GetUserById("owner-id"))
                .ReturnsAsync(new UserDto { Id = "owner-id", Email = "john@example.com", UserName = "john", FirstName = "John", LastName = "Doe" });

            context.SavingAccounts.Add(new SavingAccount 
            { 
                Id = 1, 
                AccountNumber = "123456789", 
                ClientId = "owner-id",
                Balance = 100
            });
            await context.SaveChangesAsync();

            var beneficiaryRepo = new BeneficiaryRepository(context, new NullLogger<GenericRepository<Beneficiary>>());
            var savingAccRepo = new SavingAccountRepository(context, new NullLogger<GenericRepository<SavingAccount>>());

            var handler = new CreateBeneficiaryCommandHandler(beneficiaryRepo, savingAccRepo, mockAccountService.Object);
            var command = new CreateBeneficiaryCommand { ClientId = "client-1", BeneficiaryAccountNumber = "123456789" };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeGreaterThan(0);
            var created = await context.Beneficiaries.FindAsync(result);
            created.Should().NotBeNull();
            created!.BeneficiaryName.Should().Be("John");
        }
    }
}
