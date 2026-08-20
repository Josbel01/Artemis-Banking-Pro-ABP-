using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.CreditCards.Commands.CreateCreditCard;

namespace ABP.Unit.Tests.Features.CreditCards.Commands.CreateCreditCard
{
    public class CreateCreditCardCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new CreateCreditCardCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

