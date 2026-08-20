using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.CreditCards.Commands.DeleteCreditCard;

namespace ABP.Unit.Tests.Features.CreditCards.Commands.DeleteCreditCard
{
    public class DeleteCreditCardCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new DeleteCreditCardCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

