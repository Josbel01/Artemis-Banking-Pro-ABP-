using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Loans.Queries.GetLoanById;

namespace ABP.Unit.Tests.Features.Loans.Queries.GetLoanById
{
    public class GetLoanByIdQueryValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new GetLoanByIdQueryValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

