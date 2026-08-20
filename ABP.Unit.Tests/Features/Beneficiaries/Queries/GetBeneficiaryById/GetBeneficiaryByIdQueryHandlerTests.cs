using FluentAssertions;
using ABP.Core.Application.Features.Beneficiaries.Queries.GetBeneficiaryById;
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
using AutoMapper;
using ABP.Core.Application.Mappings.EntitiesAndDtos;

namespace ABP.Unit.Tests.Features.Beneficiaries.Queries.GetBeneficiaryById
{
    public class GetBeneficiaryByIdQueryHandlerTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetBeneficiaryByIdQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_GetBeneficiaryById_{Guid.NewGuid()}")
                .Options;
                
            var mapperConfig = new MapperConfiguration(c =>
            {
                c.AddProfile<BeneficiaryMappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_Beneficiary()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            
            context.Beneficiaries.Add(new Beneficiary { Id = 1, ClientId = "client-1", BeneficiaryAccountNumber = "111" });
            await context.SaveChangesAsync();

            var repo = new BeneficiaryRepository(context, new NullLogger<GenericRepository<Beneficiary>>());
            var handler = new GetBeneficiaryByIdQueryHandler(repo, _mapper);
            var query = new GetBeneficiaryByIdQuery { Id = 1 };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.BeneficiaryAccountNumber.Should().Be("111");
        }
    }
}
