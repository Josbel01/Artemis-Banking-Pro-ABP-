using FluentAssertions;
using ABP.Core.Application.Features.Beneficiaries.Commands.DeleteBeneficiary;
using ABP.Core.Domain.Entities;
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
using ABP.Core.Application.Exceptions;

namespace ABP.Unit.Tests.Features.Beneficiaries.Commands.DeleteBeneficiary
{
    public class DeleteBeneficiaryCommandHandlerTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;

        public DeleteBeneficiaryCommandHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_DeleteBeneficiary_{Guid.NewGuid()}")
                .Options;
        }

        [Fact]
        public async Task Handle_Should_Delete_Beneficiary_When_Exists()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            
            context.Beneficiaries.Add(new Beneficiary 
            { 
                Id = 1, 
                ClientId = "client-1", 
                BeneficiaryAccountNumber = "123456789" 
            });
            await context.SaveChangesAsync();

            var beneficiaryRepo = new BeneficiaryRepository(context, new NullLogger<GenericRepository<Beneficiary>>());
            var handler = new DeleteBeneficiaryCommandHandler(beneficiaryRepo);
            var command = new DeleteBeneficiaryCommand { Id = 1 };

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            var deleted = await context.Beneficiaries.FindAsync(1);
            deleted.Should().BeNull();
        }
    }
}
