using AutoMapper;
using FluentAssertions;
using ABP.Core.Application.Dtos.Beneficiaries;
using ABP.Core.Application.Mappings.EntitiesAndDtos;
using ABP.Core.Application.Services;
using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ABP.Unit.Tests.Services
{
    public class BeneficiaryServiceTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public BeneficiaryServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_BeneficiaryService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BeneficiaryMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private BeneficiaryService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<BeneficiaryService>());

            var factoryRepMoq = new Mock<ILoggerFactory>();
            factoryRepMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<GenericRepository<Beneficiary>>());

            var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new BeneficiaryRepository(context, new NullLogger<GenericRepository<Beneficiary>>());
            return new BeneficiaryService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Beneficiary()
        {
            // Arrange
            var service = CreateService();
            var dto = new BeneficiaryDto { Id = 0, ClientId = "client1", BeneficiaryAccountNumber = "123456789", BeneficiaryName = "John" };

            // Act
            var result = await service.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Beneficiary()
        {
            // Arrange
            var context = new ArtemisBankingAppContext(_dbOptions);
            var beneficiary = new Beneficiary { Id = 0, ClientId = "client1", BeneficiaryAccountNumber = "111111111", BeneficiaryName = "Jane" };
            context.Beneficiaries.Add(beneficiary);
            await context.SaveChangesAsync();
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(beneficiary.Id);

            // Assert
            result.Should().NotBeNull();
            result!.BeneficiaryName.Should().Be("Jane");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_List()
        {
            // Arrange
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.Beneficiaries.AddRange(
                new Beneficiary { Id = 0, ClientId = "c1", BeneficiaryAccountNumber = "111", BeneficiaryName = "A" },
                new Beneficiary { Id = 0, ClientId = "c2", BeneficiaryAccountNumber = "222", BeneficiaryName = "B" }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Beneficiary()
        {
            // Arrange
            var context = new ArtemisBankingAppContext(_dbOptions);
            var beneficiary = new Beneficiary { Id = 0, ClientId = "c1", BeneficiaryAccountNumber = "999", BeneficiaryName = "Del" };
            context.Beneficiaries.Add(beneficiary);
            await context.SaveChangesAsync();
            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(beneficiary.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllByClientIdAsync_Should_Return_Filtered_Beneficiaries()
        {
            // Arrange
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.Beneficiaries.AddRange(
                new Beneficiary { Id = 0, ClientId = "clientA", BeneficiaryAccountNumber = "aaa", BeneficiaryName = "X" },
                new Beneficiary { Id = 0, ClientId = "clientB", BeneficiaryAccountNumber = "bbb", BeneficiaryName = "Y" },
                new Beneficiary { Id = 0, ClientId = "clientA", BeneficiaryAccountNumber = "ccc", BeneficiaryName = "Z" }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            // Act
            var result = await service.GetAllByClientIdAsync("clientA");

            // Assert
            result.Should().HaveCount(2);
            result.All(b => b.ClientId == "clientA").Should().BeTrue();
        }

        [Fact]
        public async Task GetAllByClientIdAsync_Should_Return_Empty_When_No_Match()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetAllByClientIdAsync("nonExistentClient");

            // Assert
            result.Should().BeEmpty();
        }
    }
}

