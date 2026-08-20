using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Transactions.Queries.GetTransactionById;

namespace ABP.Unit.Tests.Features.Transactions.Queries.GetTransactionById
{
    public class GetTransactionByIdQueryValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new GetTransactionByIdQueryValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

