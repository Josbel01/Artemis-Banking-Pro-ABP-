using ABP.Core.Application.Helpers;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace ABP.Unit.Tests.Features
{
    public class LoanAmortizationCalculatorTests
    {
        [Fact]
        public void CalculateMonthlyPayment_WithPositiveRate_ReturnsCorrectValue()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12.0m;
            int term = 12;

            // Act
            var result = LoanAmortizationCalculator.CalculateMonthlyPayment(principal, annualRate, term);

            // Assert
            result.Should().BeGreaterThan(0);
            result.Should().Be(8884.88m);
        }

        [Fact]
        public void CalculateMonthlyPayment_WithZeroRate_ReturnsPrincipalDividedByTerm()
        {
            // Arrange
            decimal principal = 60000m;
            decimal annualRate = 0m;
            int term = 12;

            // Act
            var result = LoanAmortizationCalculator.CalculateMonthlyPayment(principal, annualRate, term);

            // Assert
            result.Should().Be(5000.00m);
        }

        [Fact]
        public void CalculateMonthlyPayment_WithHigherRate_ReturnsHigherPayment()
        {
            // Arrange
            decimal principal = 100000m;
            int term = 24;

            // Act
            var lowRate = LoanAmortizationCalculator.CalculateMonthlyPayment(principal, 10m, term);
            var highRate = LoanAmortizationCalculator.CalculateMonthlyPayment(principal, 20m, term);

            // Assert
            highRate.Should().BeGreaterThan(lowRate);
        }

        [Fact]
        public void GenerateAmortizationSchedule_ReturnsCorrectNumberOfInstallments()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int term = 12;
            var startDate = new DateTime(2026, 1, 1);

            // Act
            var schedule = LoanAmortizationCalculator.GenerateAmortizationSchedule(principal, annualRate, term, startDate);

            // Assert
            schedule.Should().HaveCount(12);
        }

        [Fact]
        public void GenerateAmortizationSchedule_FirstInstallmentHasCorrectValues()
        {
            // Arrange
            decimal principal = 100000m;
            decimal annualRate = 12m;
            int term = 12;
            var startDate = new DateTime(2026, 1, 1);

            // Act
            var schedule = LoanAmortizationCalculator.GenerateAmortizationSchedule(principal, annualRate, term, startDate);
            var first = schedule[0];

            // Assert
            first.InstallmentNumber.Should().Be(1);
            first.DueDate.Should().Be(new DateTime(2026, 2, 1));
            first.PaymentStatus.Should().Be(PaymentStatus.Pending);
            first.IsLate.Should().BeFalse();
            first.InterestAmount.Should().Be(1000m);
            first.InstallmentAmount.Should().Be(first.InterestAmount + first.CapitalAmount);
        }

        [Fact]
        public void GenerateAmortizationSchedule_LastInstallmentClosesBalanceToZero()
        {
            // Arrange
            decimal principal = 50000m;
            decimal annualRate = 15m;
            int term = 6;
            var startDate = new DateTime(2026, 1, 1);

            // Act
            var schedule = LoanAmortizationCalculator.GenerateAmortizationSchedule(principal, annualRate, term, startDate);
            var last = schedule.Last();

            // Assert
            last.InstallmentNumber.Should().Be(6);
            last.CapitalAmount.Should().BeGreaterThan(0);
            last.PaymentStatus.Should().Be(PaymentStatus.Pending);
        }

        [Fact]
        public void GenerateAmortizationSchedule_DatesAreMonthly()
        {
            // Arrange
            var startDate = new DateTime(2026, 3, 15);

            // Act
            var schedule = LoanAmortizationCalculator.GenerateAmortizationSchedule(10000m, 10m, 6, startDate);

            // Assert
            for (int i = 0; i < schedule.Count; i++)
            {
                schedule[i].DueDate.Should().Be(startDate.AddMonths(i + 1));
            }
        }

        [Fact]
        public void GenerateAmortizationSchedule_AllInstallmentsStartAsPending()
        {
            // Act
            var schedule = LoanAmortizationCalculator.GenerateAmortizationSchedule(100000m, 12m, 24, DateTime.Now);

            // Assert
            schedule.Should().AllSatisfy(inst =>
            {
                inst.PaymentStatus.Should().Be(PaymentStatus.Pending);
                inst.IsLate.Should().BeFalse();
            });
        }

        [Fact]
        public void GenerateAmortizationSchedule_SumOfCapitalEqualsPrincipal()
        {
            // Arrange
            decimal principal = 200000m;

            // Act
            var schedule = LoanAmortizationCalculator.GenerateAmortizationSchedule(principal, 10m, 36, DateTime.Now);

            // Assert
            var totalCapital = schedule.Sum(i => i.CapitalAmount);
            totalCapital.Should().BeApproximately(principal, 1.0m);
        }
    }
}
