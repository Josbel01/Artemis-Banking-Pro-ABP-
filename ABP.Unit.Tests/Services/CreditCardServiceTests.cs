using AutoMapper;
using FluentAssertions;
using ABP.Core.Application.Dtos.CreditCards;
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
    public class CreditCardServiceTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public CreditCardServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_CreditCardService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CreditCardMappingProfile>();
                cfg.AddProfile<CardTransactionMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private CreditCardService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<CreditCardService>());

            var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new CreditCardRepository(context, new NullLogger<GenericRepository<CreditCard>>());
            return new CreditCardService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_CreditCard()
        {
            var service = CreateService();
            var dto = new CreditCardDto { Id = 0, ClientId = "c1", CardNumber = "1234567812345678", CreditLimit = 5000 };

            var result = await service.AddAsync(dto);

            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Card()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var card = new CreditCard { Id = 0, ClientId = "c1", CardNumber = "1111222233334444", CreditLimit = 3000 };
            context.CreditCards.Add(card);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByIdAsync(card.Id);

            result.Should().NotBeNull();
            result!.CardNumber.Should().Be("1111222233334444");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Cards()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.CreditCards.AddRange(
                new CreditCard { Id = 0, ClientId = "c1", CardNumber = "aaaa", CreditLimit = 1000 },
                new CreditCard { Id = 0, ClientId = "c2", CardNumber = "bbbb", CreditLimit = 2000 }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Card()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var card = new CreditCard { Id = 0, ClientId = "c1", CardNumber = "del1", CreditLimit = 500 };
            context.CreditCards.Add(card);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.DeleteAsync(card.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Card()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var card = new CreditCard { Id = 0, ClientId = "c1", CardNumber = "upd1", CreditLimit = 1000 };
            context.CreditCards.Add(card);
            await context.SaveChangesAsync();
            var service = CreateService();

            var dto = new CreditCardDto { Id = card.Id, ClientId = "c1", CardNumber = "upd1", CreditLimit = 5000 };
            var result = await service.UpdateAsync(dto, card.Id);

            result.Should().NotBeNull();
            result!.CreditLimit.Should().Be(5000);
        }
    }
}

