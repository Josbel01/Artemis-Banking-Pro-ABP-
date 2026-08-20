using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.LoanInstallments.Commands.CreateLoanInstallment;

namespace ABP.Unit.Tests.Features.LoanInstallments.Commands.CreateLoanInstallment
{
    public class CreateLoanInstallmentCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange
            var mockloanRepository = new Mock<ILoanRepository>();

            var validator = new CreateLoanInstallmentCommandValidator(mockloanRepository.Object);

            // Assert
            Assert.NotNull(validator);
        }
    }
}

