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
    public class LoanInstallmentServiceTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public LoanInstallmentServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_LoanInstallmentService_{Guid.NewGuid()}")
                .Options;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LoanInstallmentMappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        private LoanInstallmentService CreateService()
        {
            var factoryMoq = new Mock<ILoggerFactory>();
            factoryMoq.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(new NullLogger<LoanInstallmentService>());

            var context = new ArtemisBankingAppContext(_dbOptions);
            var repo = new LoanInstallmentRepository(context, new NullLogger<GenericRepository<LoanInstallment>>());
            return new LoanInstallmentService(repo, _mapper, factoryMoq.Object);
        }

        [Fact]
        public async Task AddAsync_Should_Add_Installment()
        {
            var service = CreateService();
            var dto = new LoanInstallmentDto { Id = 0, LoanId = 1, InstallmentNumber = 1, InstallmentAmount = 100, InterestAmount = 5, DueDate = DateTime.Now, PaymentStatus = ABP.Core.Domain.Common.Enums.PaymentStatus.Paid };

            var result = await service.AddAsync(dto);

            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Installment()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var inst = new LoanInstallment { Id = 0, LoanId = 1, InstallmentNumber = 2, InstallmentAmount = 50, InterestAmount = 2, DueDate = DateTime.Now, PaymentStatus = ABP.Core.Domain.Common.Enums.PaymentStatus.Pending };
            context.LoanInstallments.Add(inst);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetByIdAsync(inst.Id);

            result.Should().NotBeNull();
            result!.InstallmentNumber.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            context.LoanInstallments.AddRange(
                new LoanInstallment { Id = 0, LoanId = 1, InstallmentNumber = 1, InstallmentAmount = 10, InterestAmount = 1, DueDate = DateTime.Now, PaymentStatus = ABP.Core.Domain.Common.Enums.PaymentStatus.Paid },
                new LoanInstallment { Id = 0, LoanId = 1, InstallmentNumber = 2, InstallmentAmount = 20, InterestAmount = 2, DueDate = DateTime.Now, PaymentStatus = ABP.Core.Domain.Common.Enums.PaymentStatus.Paid }
            );
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_Should_Delete_Installment()
        {
            var context = new ArtemisBankingAppContext(_dbOptions);
            var inst = new LoanInstallment { Id = 0, LoanId = 1, InstallmentNumber = 3, InstallmentAmount = 30, InterestAmount = 3, DueDate = DateTime.Now, PaymentStatus = ABP.Core.Domain.Common.Enums.PaymentStatus.Pending };
            context.LoanInstallments.Add(inst);
            await context.SaveChangesAsync();
            var service = CreateService();

            var result = await service.DeleteAsync(inst.Id);

            result.Should().BeTrue();
        }
    }
}
