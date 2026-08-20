using FluentAssertions;
using ABP.Core.Application.Features.Beneficiaries.Queries.GetAllBeneficiaries;
using ABP.Core.Domain.Entities;
using ABP.Infrastructure.Persistence.Contexts;
using ABP.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AutoMapper;
using ABP.Core.Application.Mappings.EntitiesAndDtos;

namespace ABP.Unit.Tests.Features.Beneficiaries.Queries.GetAllBeneficiaries
{
    public class GetAllBeneficiariesQueryHandlerTests
    {
        private readonly DbContextOptions<ArtemisBankingAppContext> _dbOptions;
        private readonly IMapper _mapper;

        public GetAllBeneficiariesQueryHandlerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ArtemisBankingAppContext>()
                .UseInMemoryDatabase($"TestDb_GetAllBeneficiaries_{Guid.NewGuid()}")
                .Options;

            var mapperConfig = new MapperConfiguration(c =>
            {
                c.AddProfile<BeneficiaryMappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public async Task Handle_Should_Return_All_Beneficiaries()
        {
            // Arrange
            using var context = new ArtemisBankingAppContext(_dbOptions);
            
            context.Beneficiaries.Add(new Beneficiary { Id = 1, ClientId = "client-1", BeneficiaryAccountNumber = "111" });
            context.Beneficiaries.Add(new Beneficiary { Id = 2, ClientId = "client-2", BeneficiaryAccountNumber = "222" });
            await context.SaveChangesAsync();

            var repo = new BeneficiaryRepository(context, new NullLogger<GenericRepository<Beneficiary>>());
            var handler = new GetAllBeneficiariesQueryHandler(repo, _mapper);
            var query = new GetAllBeneficiariesQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.First(x => x.Id == 1).BeneficiaryAccountNumber.Should().Be("111");
        }
    }
}
