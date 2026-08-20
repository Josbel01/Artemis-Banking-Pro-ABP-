using FluentAssertions;
using ABP.Core.Application.Features.Beneficiaries.Commands.UpdateBeneficiary;
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

namespace ABP.Unit.Tests.Features.Beneficiaries.Commands.UpdateBeneficiary
{
    public class UpdateBeneficiaryCommandHandlerTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public UpdateBeneficiaryCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_UpdateBeneficiary_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Update_Beneficiary_When_Valid()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            
            var mockAccountService = new Mock<IBaseAccountService>();
            mockAccountService.Setup(x => x.GetUserById("owner-id"))
                .ReturnsAsync(new UserDto { Id = "owner-id", Email = "jane@example.com", UserName = "jane", FirstName = "Jane", LastName = "Doe" });

            context.Beneficiaries.Add(new Beneficiary 
            { 
                Id = 1, 
                ClientId = "client-1", 
                BeneficiaryAccountNumber = "old-acc" 
            });

            context.SavingAccounts.Add(new SavingAccount 
            { 
                Id = 1, 
                AccountNumber = "new-acc", 
                ClientId = "owner-id",
                Balance = 100
            });
            await context.SaveChangesAsync();

            var beneficiaryRepo = new BeneficiaryRepository(context, new NullLogger<GenericRepository<Beneficiary>>());
            var savingAccRepo = new SavingAccountRepository(context, new NullLogger<GenericRepository<SavingAccount>>());

            var handler = new UpdateBeneficiaryCommandHandler(beneficiaryRepo, savingAccRepo, mockAccountService.Object);
            var command = new UpdateBeneficiaryCommand { Id = 1, ClientId = "client-1", BeneficiaryAccountNumber = "new-acc" };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var updated = await context.Beneficiaries.FindAsync(1);
            updated.Should().NotBeNull();
            updated!.BeneficiaryAccountNumber.Should().Be("new-acc");
            updated.BeneficiaryName.Should().Be("Jane");
        }
    }
}
