using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.CardTransactions.Commands.CreateCardTransaction;

namespace ABP.Unit.Tests.Features.CardTransactions.Commands.CreateCardTransaction
{
    public class CreateCardTransactionCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange
            var mockcreditCardRepository = new Mock<ICreditCardRepository>();

            var validator = new CreateCardTransactionCommandValidator(mockcreditCardRepository.Object);

            // Assert
            Assert.NotNull(validator);
        }
    }
}

