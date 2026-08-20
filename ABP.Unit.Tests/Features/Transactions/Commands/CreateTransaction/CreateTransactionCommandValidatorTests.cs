using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Transactions.Commands.CreateTransaction;

namespace ABP.Unit.Tests.Features.Transactions.Commands.CreateTransaction
{
    public class CreateTransactionCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange
            var mocksavingAccountRepository = new Mock<ISavingAccountRepository>();

            var validator = new CreateTransactionCommandValidator(mocksavingAccountRepository.Object);

            // Assert
            Assert.NotNull(validator);
        }
    }
}

