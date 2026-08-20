using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Commerces.Queries.GetCommerceById;

namespace ABP.Unit.Tests.Features.Commerces.Queries.GetCommerceById
{
    public class GetCommerceByIdQueryValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new GetCommerceByIdQueryValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

