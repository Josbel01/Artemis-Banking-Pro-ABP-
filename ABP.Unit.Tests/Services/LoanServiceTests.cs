using AutoMapper;
using FluentAssertions;
using ABP.Core.Application.Dtos.Loans;
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
    public class LoanServiceTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public LoanServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_LoanService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LoanMappingProfile>();
                cfg.AddProfile<LoanInstallmentMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private LoanService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<LoanService>());

            var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new LoanRepository(context, new NullLogger<GenericRepository<Loan>>());
            return new LoanService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Loan()
        {
            var service = CreateService();
            var dto = new LoanDto { Id = 0, ClientId = "c1", LoanNumber = "LN001", AmountApproved = 10000, AnnualInterestRate = 5.5m };

            var result = await service.AddAsync(dto);

            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Loan()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var loan = new Loan { Id = 0, ClientId = "c1", LoanNumber = "LN002", AmountApproved = 5000, AnnualInterestRate = 3m };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByIdAsync(loan.Id);

            result.Should().NotBeNull();
            result!.LoanNumber.Should().Be("LN002");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Loans()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.Loans.AddRange(
                new Loan { Id = 0, ClientId = "c1", LoanNumber = "LN010", AmountApproved = 1000, AnnualInterestRate = 2m },
                new Loan { Id = 0, ClientId = "c2", LoanNumber = "LN011", AmountApproved = 2000, AnnualInterestRate = 3m }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Loan()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var loan = new Loan { Id = 0, ClientId = "c1", LoanNumber = "LN099", AmountApproved = 500, AnnualInterestRate = 1m };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.DeleteAsync(loan.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetByLoanNumberAsync_Should_Return_Loan_When_Exists()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var loan = new Loan { Id = 0, ClientId = "c1", LoanNumber = "LN500", AmountApproved = 7500, AnnualInterestRate = 4m };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByLoanNumberAsync("LN500");

            result.Should().NotBeNull();
            result!.AmountApproved.Should().Be(7500);
        }

        [Fact]
        public async Task GetByLoanNumberAsync_Should_Return_Null_When_Not_Found()
        {
            var service = CreateService();

            var result = await service.GetByLoanNumberAsync("NONEXISTENT");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllByClientIdAsync_Should_Return_Filtered_Loans()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.Loans.AddRange(
                new Loan { Id = 0, ClientId = "clientX", LoanNumber = "LNX1", AmountApproved = 100, AnnualInterestRate = 1m },
                new Loan { Id = 0, ClientId = "clientY", LoanNumber = "LNY1", AmountApproved = 200, AnnualInterestRate = 2m },
                new Loan { Id = 0, ClientId = "clientX", LoanNumber = "LNX2", AmountApproved = 300, AnnualInterestRate = 3m }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetAllByClientIdAsync("clientX");

            result.Should().HaveCount(2);
        }
    }
}
