using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.SavingAccounts.Queries.GetSavingAccountById;

namespace ABP.Unit.Tests.Features.SavingAccounts.Queries.GetSavingAccountById
{
    public class GetSavingAccountByIdQueryValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new GetSavingAccountByIdQueryValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

