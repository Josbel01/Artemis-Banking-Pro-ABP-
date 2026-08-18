using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Infrastructure.Identity.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FunctionApp1;

public class DailyLoanDelinquencyReview
{
    private readonly ILogger<DailyLoanDelinquencyReview> _logger;
    private readonly ILoanInstallmentService _loanInstallmentService;
    private readonly ILoanService _loanService;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;

    public DailyLoanDelinquencyReview(ILogger<DailyLoanDelinquencyReview> logger,
        ILoanInstallmentService loanInstallmentService, ILoanService loanService,
        IEmailService emailService, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _loanInstallmentService = loanInstallmentService;
        _loanService = loanService;
        _emailService = emailService;
        _userManager = userManager;
    }

    [Function("DailyLoanDelinquencyReview")]
    public async Task Run([TimerTrigger("%TimeTrigger%")] TimerInfo timer)
    {
        if (timer.IsPastDue)
            _logger.LogWarning("The daily loan delinquency review ran later than scheduled.");

        var today = DateTime.UtcNow.Date;
        var overdueInstallments = (await _loanInstallmentService.GetAllAsync())
            .Where(installment => installment.PaymentStatus != PaymentStatus.Paid
                && installment.PendingAmount > 0
                && installment.DueDate.Date < today)
            .ToList();

        foreach (var installment in overdueInstallments)
        {
            if (!installment.IsLate)
            {
                installment.IsLate = true;
                await _loanInstallmentService.UpdateAsync(installment, installment.Id);
            }

            var loan = await _loanService.GetByIdAsync(installment.LoanId);
            if (loan is null || loan.ClientPaymentStatus == "Atrasado")
                continue;

            loan.ClientPaymentStatus = "Atrasado";
            await _loanService.UpdateAsync(loan, loan.Id);

            var client = await _userManager.FindByIdAsync(loan.ClientId);
            if (!string.IsNullOrWhiteSpace(client?.Email))
            {
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = client.Email,
                    Subject = "Cuota de préstamo pendiente",
                    HtmlBody = "Tiene una cuota de préstamo vencida. Por favor, revise su cuenta para regularizar el pago."
                });
            }
        }

        _logger.LogInformation("Daily loan review completed. Overdue installments: {OverdueCount}", overdueInstallments.Count);
    }
}
