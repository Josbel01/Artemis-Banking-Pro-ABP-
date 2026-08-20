using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Beneficiaries.Commands.UpdateBeneficiary;

namespace ABP.Unit.Tests.Features.Beneficiaries.Commands.UpdateBeneficiary
{
    public class UpdateBeneficiaryCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new UpdateBeneficiaryCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

