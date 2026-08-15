using System;
using System.Collections.Generic;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;

namespace ABP.Core.Application.Helpers
{
    public static class LoanAmortizationCalculator
    {
        // Sistema francés: cuota fija, la proporción capital/interés cambia cada mes
        public static decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termInMonths)
        {
            decimal monthlyRate = annualInterestRate / 100 / 12;

            if (monthlyRate == 0)
                return Math.Round(principal / termInMonths, 2);

            double rate = (double)monthlyRate;
            double factor = Math.Pow(1 + rate, termInMonths);
            decimal quota = principal * (monthlyRate * (decimal)factor) / ((decimal)factor - 1);

            return Math.Round(quota, 2);
        }

        public static List<LoanInstallment> GenerateAmortizationSchedule(
            decimal principal, decimal annualInterestRate, int termInMonths, DateTime startDate)
        {
            var installments = new List<LoanInstallment>();
            decimal monthlyRate = annualInterestRate / 100 / 12;
            decimal quota = CalculateMonthlyPayment(principal, annualInterestRate, termInMonths);
            decimal balance = principal;

            for (int i = 1; i <= termInMonths; i++)
            {
                decimal interest = Math.Round(balance * monthlyRate, 2);
                decimal capital = quota - interest;

                // En la última cuota, ajusta para que el balance cierre exacto en 0
                if (i == termInMonths)
                    capital = balance;

                balance -= capital;
                if (balance < 0) balance = 0;

                installments.Add(new LoanInstallment
                {
                    InstallmentNumber = i,
                    DueDate = startDate.AddMonths(i),
                    InstallmentAmount = interest + capital,
                    InterestAmount = interest,
                    CapitalAmount = capital,
                    PendingAmount = interest + capital, // Start with full amount pending
                    PaymentStatus = PaymentStatus.Pending,
                    IsLate = false
                });
            }

            return installments;
        }
    }
}
