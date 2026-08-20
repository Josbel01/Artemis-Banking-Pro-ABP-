using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.CardTransactions.Queries.GetCardTransactionById;

namespace ABP.Unit.Tests.Features.CardTransactions.Queries.GetCardTransactionById
{
    public class GetCardTransactionByIdQueryValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new GetCardTransactionByIdQueryValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

