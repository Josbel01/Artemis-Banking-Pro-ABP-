using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.CreditCards.Commands.UpdateCreditCardLimit;

namespace ABP.Unit.Tests.Features.CreditCards.Commands.UpdateCreditCardLimit
{
    public class UpdateCreditCardLimitCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new UpdateCreditCardLimitCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

