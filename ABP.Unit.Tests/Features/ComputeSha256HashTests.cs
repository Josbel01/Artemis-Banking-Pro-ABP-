using ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment;
using FluentAssertions;
using Xunit;

namespace ABP.Unit.Tests.Features
{
    public class ComputeSha256HashTests
    {
        [Fact]
        public void ComputeSha256Hash_WithSameInput_ReturnsSameHash()
        {
            // Arrange
            string input = "123";

            // Act
            var hash1 = ProcessPaymentCommandHandler.ComputeSha256Hash(input);
            var hash2 = ProcessPaymentCommandHandler.ComputeSha256Hash(input);

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void ComputeSha256Hash_WithDifferentInput_ReturnsDifferentHash()
        {
            // Act
            var hash1 = ProcessPaymentCommandHandler.ComputeSha256Hash("123");
            var hash2 = ProcessPaymentCommandHandler.ComputeSha256Hash("456");

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void ComputeSha256Hash_Returns64CharacterHexString()
        {
            // Act
            var hash = ProcessPaymentCommandHandler.ComputeSha256Hash("789");

            // Assert
            hash.Should().HaveLength(64);
            hash.Should().MatchRegex("^[0-9a-f]{64}$");
        }

        [Fact]
        public void ComputeSha256Hash_WithEmptyString_ReturnsValidHash()
        {
            // Act
            var hash = ProcessPaymentCommandHandler.ComputeSha256Hash("");

            // Assert
            hash.Should().HaveLength(64);
            hash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ComputeSha256Hash_WithCvcFormat_ReturnsConsistentHash()
        {
            // Arrange
            string[] cvcs = { "000", "111", "999", "123", "456", "789" };

            // Act & Assert
            foreach (var cvc in cvcs)
            {
                var hash = ProcessPaymentCommandHandler.ComputeSha256Hash(cvc);
                hash.Should().HaveLength(64, $"CVC '{cvc}' should produce a 64-char hash");
                ProcessPaymentCommandHandler.ComputeSha256Hash(cvc).Should().Be(hash);
            }
        }
    }
}
