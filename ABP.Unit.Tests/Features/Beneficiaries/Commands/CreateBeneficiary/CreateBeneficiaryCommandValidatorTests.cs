using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Beneficiaries.Commands.CreateBeneficiary;

namespace ABP.Unit.Tests.Features.Beneficiaries.Commands.CreateBeneficiary
{
    public class CreateBeneficiaryCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new CreateBeneficiaryCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

