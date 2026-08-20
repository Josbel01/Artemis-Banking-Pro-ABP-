using AutoMapper;
using FluentAssertions;
using ABP.Core.Application.Dtos.CardTransactions;
using ABP.Core.Application.Mappings.EntitiesAndDtos;
using ABP.Core.Application.Services;
using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading.Tasks;

namespace ABP.Unit.Tests.Services
{
    public class CardTransactionServiceTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public CardTransactionServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_CardTransactionService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CardTransactionMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private CardTransactionService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<CardTransactionService>());

            var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new CardTransactionRepository(context, new NullLogger<GenericRepository<CardTransaction>>());
            return new CardTransactionService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Transaction()
        {
            var service = CreateService();
            var dto = new CardTransactionDto { Id = 0, CreditCardId = 1, Amount = 100, CommerceName = "Test" };

            var result = await service.AddAsync(dto);

            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Transaction()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var tx = new CardTransaction { Id = 0, CreditCardId = 1, Amount = 50, CommerceName = "Find" };
            context.CardTransactions.Add(tx);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByIdAsync(tx.Id);

            result.Should().NotBeNull();
            result!.CommerceName.Should().Be("Find");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.CardTransactions.AddRange(
                new CardTransaction { Id = 0, CreditCardId = 1, Amount = 10, CommerceName = "A" },
                new CardTransaction { Id = 0, CreditCardId = 1, Amount = 20, CommerceName = "B" }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Transaction()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var tx = new CardTransaction { Id = 0, CreditCardId = 1, Amount = 30, CommerceName = "Del" };
            context.CardTransactions.Add(tx);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.DeleteAsync(tx.Id);

            result.Should().BeTrue();
        }
    }
}
