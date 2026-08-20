using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Beneficiaries.Queries.GetBeneficiaryById;

namespace ABP.Unit.Tests.Features.Beneficiaries.Queries.GetBeneficiaryById
{
    public class GetBeneficiaryByIdQueryValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new GetBeneficiaryByIdQueryValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

