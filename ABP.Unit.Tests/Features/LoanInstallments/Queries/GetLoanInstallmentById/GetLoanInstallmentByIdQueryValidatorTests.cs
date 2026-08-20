using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.LoanInstallments.Queries.GetLoanInstallmentById;

namespace ABP.Unit.Tests.Features.LoanInstallments.Queries.GetLoanInstallmentById
{
    public class GetLoanInstallmentByIdQueryValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new GetLoanInstallmentByIdQueryValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

