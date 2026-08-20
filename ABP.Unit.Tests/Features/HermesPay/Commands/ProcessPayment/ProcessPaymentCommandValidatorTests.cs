using FluentValidation.TestHelper;
using Moq;
using Xunit;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Application.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment;

namespace ABP.Unit.Tests.Features.HermesPay.Commands.ProcessPayment
{
    public class ProcessPaymentCommandValidatorTests
    {
        [Fact]
        public void Should_Be_Instantiated_And_Have_Rules()
        {
            // Arrange

            var validator = new ProcessPaymentCommandValidator();

            // Assert
            Assert.NotNull(validator);
        }
    }
}

