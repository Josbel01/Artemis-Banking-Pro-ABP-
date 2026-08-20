using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.Loans.Commands.CreateLoan;

namespace ABP.Unit.Tests.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new CreateLoanCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

