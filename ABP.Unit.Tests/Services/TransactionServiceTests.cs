using AutoMapper;
using FluentAssertions;
using ABP.Core.Application.Dtos.Transactions;
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
    public class TransactionServiceTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public TransactionServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_TransactionService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TransactionMappingProfile>();
                cfg.AddProfile<SavingAccountMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private TransactionService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<TransactionService>());

            var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new TransactionRepository(context, new NullLogger<GenericRepository<Transaction>>());
            var savingRepo = new SavingAccountRepository(context, new NullLogger<GenericRepository<SavingAccount>>());
            var creditRepo = new CreditCardRepository(context, new NullLogger<GenericRepository<CreditCard>>());
            var cardTxRepo = new CardTransactionRepository(context, new NullLogger<GenericRepository<CardTransaction>>());
            var loanRepo = new LoanRepository(context, new NullLogger<GenericRepository<Loan>>());
            var loanInstRepo = new LoanInstallmentRepository(context, new NullLogger<GenericRepository<LoanInstallment>>());
            
            return new TransactionService(repo, savingRepo, creditRepo, cardTxRepo, loanRepo, loanInstRepo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Transaction()
        {
            var service = CreateService();
            var dto = new TransactionDto { Id = 0, SavingAccountId = 1, Amount = 100, Type = ABP.Core.Domain.Common.Enums.TransactionType.Transfer };

            var result = await service.AddAsync(dto);

            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Transaction()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.SavingAccounts.Add(new SavingAccount { Id = 1, AccountNumber = "123", ClientId = "c1" });
            var tx = new Transaction { Id = 0, SavingAccountId = 1, Amount = 50, Type = ABP.Core.Domain.Common.Enums.TransactionType.Credit };
            context.Transactions.Add(tx);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByIdAsync(tx.Id);

            result.Should().NotBeNull();
            result!.Amount.Should().Be(50);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.SavingAccounts.AddRange(
                new SavingAccount { Id = 1, AccountNumber = "123", ClientId = "c1" },
                new SavingAccount { Id = 3, AccountNumber = "456", ClientId = "c2" }
            );
            context.Transactions.AddRange(
                new Transaction { Id = 0, SavingAccountId = 1, Amount = 10, Type = ABP.Core.Domain.Common.Enums.TransactionType.Transfer },
                new Transaction { Id = 0, SavingAccountId = 3, Amount = 20, Type = ABP.Core.Domain.Common.Enums.TransactionType.Credit }
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
            context.SavingAccounts.Add(new SavingAccount { Id = 1, AccountNumber = "123", ClientId = "c1" });
            var tx = new Transaction { Id = 0, SavingAccountId = 1, Amount = 30, Type = ABP.Core.Domain.Common.Enums.TransactionType.Transfer };
            context.Transactions.Add(tx);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.DeleteAsync(tx.Id);

            result.Should().BeTrue();
        }
    }
}
