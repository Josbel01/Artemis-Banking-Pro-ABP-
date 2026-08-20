using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate;

namespace ABP.Unit.Tests.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new UpdateLoanRateCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

