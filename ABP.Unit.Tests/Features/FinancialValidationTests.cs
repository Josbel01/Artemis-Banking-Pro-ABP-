using ABP.Core.Application.Features.HermesPay.Commands.ProcessPayment;
using ABP.Core.Application.Features.Loans.Commands.CreateLoan;
using ABP.Core.Application.Helpers;
using ABP.Core.Domain.Common.Enums;
using Xunit;

namespace ABP.Unit.Tests.Features;

public class FinancialValidationTests
{
    [Fact]
    public void CalculateMonthlyPayment_WithoutInterest_DividesCapitalEqually()
    {
        var payment = LoanAmortizationCalculator.CalculateMonthlyPayment(1_200m, 0m, 12);

        Assert.Equal(100m, payment);
    }

    [Fact]
    public void GenerateAmortizationSchedule_CoversTheApprovedCapital()
    {
        var schedule = LoanAmortizationCalculator.GenerateAmortizationSchedule(12_000m, 12m, 12, new DateTime(2026, 1, 1));

        Assert.Equal(12, schedule.Count);
        Assert.Equal(12_000m, schedule.Sum(installment => installment.CapitalAmount));
        Assert.All(schedule, installment => Assert.Equal(PaymentStatus.Pending, installment.PaymentStatus));
        Assert.Equal(0m, schedule.Last().PendingAmount - schedule.Last().InstallmentAmount);
    }

    [Theory]
    [InlineData("1234567890123456", "12", "2027", "123", true)]
    [InlineData("123", "12", "2027", "123", false)]
    [InlineData("1234567890123456", "13", "2027", "123", false)]
    [InlineData("1234567890123456", "12", "20", "123", false)]
    [InlineData("1234567890123456", "12", "2027", "12A", false)]
    public void ProcessPaymentValidator_ValidatesSensitivePaymentFields(string number, string month, string year, string cvc, bool valid)
    {
        var command = new ProcessPaymentCommand
        {
            CommerceId = 1,
            CardNumber = number,
            MonthExpirationCard = month,
            YearExpirationCard = year,
            Cvc = cvc,
            TransactionAmount = 1m
        };

        var result = new ProcessPaymentCommandValidator().Validate(command);

        Assert.Equal(valid, result.IsValid);
    }

    [Fact]
    public void CreateLoanValidator_RejectsNonPositiveCapital()
    {
        var result = new CreateLoanCommandValidator().Validate(new CreateLoanCommand
        {
            ClientId = "client-1",
            CapitalAmount = 0m,
            TermInMonths = 12,
            AnnualInterestRate = 10m
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ComputeSha256Hash_IsDeterministicAndDoesNotReturnPlainText()
    {
        var first = ProcessPaymentCommandHandler.ComputeSha256Hash("123");
        var second = ProcessPaymentCommandHandler.ComputeSha256Hash("123");

        Assert.Equal(first, second);
        Assert.NotEqual("123", first);
        Assert.Equal(64, first.Length);
    }
}
