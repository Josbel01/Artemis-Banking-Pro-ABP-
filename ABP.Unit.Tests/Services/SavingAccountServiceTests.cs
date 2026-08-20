using AutoMapper;
using FluentAssertions;
using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Application.Mappings.EntitiesAndDtos;
using ABP.Core.Application.Services;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ABP.Unit.Tests.Services
{
    public class SavingAccountServiceTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public SavingAccountServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_SavingAccountService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SavingAccountMappingProfile>();
                cfg.AddProfile<TransactionMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private SavingAccountService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<SavingAccountService>());

            var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new SavingAccountRepository(context, new NullLogger<GenericRepository<SavingAccount>>());
            return new SavingAccountService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_SavingAccount()
        {
            var service = CreateService();
            var dto = new SavingAccountDto { Id = 0, ClientId = "c1", AccountNumber = "100000001", AccountType = SavingAccountType.Main, Balance = 1000 };

            var result = await service.AddAsync(dto);

            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Account()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var account = new SavingAccount { Id = 0, ClientId = "c1", AccountNumber = "200000001", AccountType = SavingAccountType.Main, Balance = 500 };
            context.SavingAccounts.Add(account);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByIdAsync(account.Id);

            result.Should().NotBeNull();
            result!.AccountNumber.Should().Be("200000001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Accounts()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.SavingAccounts.AddRange(
                new SavingAccount { Id = 0, ClientId = "c1", AccountNumber = "300000001", AccountType = SavingAccountType.Main, Balance = 100 },
                new SavingAccount { Id = 0, ClientId = "c2", AccountNumber = "300000002", AccountType = SavingAccountType.Secondary, Balance = 200 }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Account()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var account = new SavingAccount { Id = 0, ClientId = "c1", AccountNumber = "400000001", AccountType = SavingAccountType.Secondary, Balance = 0 };
            context.SavingAccounts.Add(account);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.DeleteAsync(account.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetByAccountNumberAsync_Should_Return_Account_When_Exists()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var account = new SavingAccount { Id = 0, ClientId = "c1", AccountNumber = "500000001", AccountType = SavingAccountType.Main, Balance = 750 };
            context.SavingAccounts.Add(account);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByAccountNumberAsync("500000001");

            result.Should().NotBeNull();
            result!.Balance.Should().Be(750);
        }

        [Fact]
        public async Task GetByAccountNumberAsync_Should_Return_Null_When_Not_Found()
        {
            var service = CreateService();

            var result = await service.GetByAccountNumberAsync("999999999");

            result.Should().BeNull();
        }
    }
}

