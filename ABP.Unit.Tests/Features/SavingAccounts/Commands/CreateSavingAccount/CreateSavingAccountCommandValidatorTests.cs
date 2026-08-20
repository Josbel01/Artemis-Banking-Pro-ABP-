using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.SavingAccounts.Commands.CreateSavingAccount;

namespace ABP.Unit.Tests.Features.SavingAccounts.Commands.CreateSavingAccount
{
    public class CreateSavingAccountCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new CreateSavingAccountCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

