using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ABP.Core.Domain.Interfaces;
using ABP.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Identity;
using ABP.Infrastructure.Identity.Entities;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.Email;
using System.Net;

namespace AutomaticOverduePaymentControlFunction;

public class FunctionAutomaticOverduePaymentControl(
    ILoggerFactory loggerFactory, 
    IGenericRepository<ABP.Core.Domain.Entities.Loan> loanRepository,
    IGenericRepository<ABP.Core.Domain.Entities.LoanInstallment> installmentRepository,
    UserManager<AppUser> userManager,
    IEmailService emailService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<FunctionAutomaticOverduePaymentControl>();
    private readonly IGenericRepository<ABP.Core.Domain.Entities.Loan> _loanRepository = loanRepository;
    private readonly IGenericRepository<ABP.Core.Domain.Entities.LoanInstallment> _installmentRepository = installmentRepository;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IEmailService _emailService = emailService;

    [Function("FunctionAutomaticOverduePaymentControl")]
    public async Task Run([TimerTrigger("%TimeTrigger%")] TimerInfo myTimer)
    {
        try
        {
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation("Next timer schedule at: {NextSchedule}", myTimer.ScheduleStatus.Next);
            }

            if (myTimer.IsPastDue)
            {
                _logger.LogWarning("Timer is past due!");
            }
            else
            {
                await ProcessOverduePaymentsAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while running the timer function: {Exception}", ex);
            return;
        }
        finally
        {
            _logger.LogInformation("FunctionAutomaticOverduePaymentControl completed at: {Time}", DateTime.UtcNow);
        }
    }

    [Function("FunctionAutomaticOverduePaymentControlHttp")]
    public async Task<HttpResponseData> RunHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            await ProcessOverduePaymentsAsync();
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync("Successfully processed overdue payments!");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while running the HTTP function: {Exception}", ex);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred while processing overdue payments.");
            return errorResponse;
        }
    }

    private async Task ProcessOverduePaymentsAsync()
    {
        var loans = await _loanRepository.GetAllListWithInclude(new List<string> { "LoanInstallments" });
        var activeLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();

        if (activeLoans.Count == 0)
        {
            _logger.LogInformation("No active loans found.");
            return;
        }

        int updatedLoansCount = 0;
        int updatedInstallmentsCount = 0;
        var emailNotifications = new List<(string Email, string Message)>();

        foreach (var loan in activeLoans)
        {
            bool loanChanged = false;
            bool newlyLateInstallment = false;
            
            foreach (var inst in loan.LoanInstallments)
            {
                bool instChanged = false;
                
                if (inst.DueDate < DateTime.Now.Date && inst.PaymentStatus != PaymentStatus.Paid && !inst.IsLate)
                {
                    inst.IsLate = true;
                    instChanged = true;
                    newlyLateInstallment = true;
                }
                else if (inst.PaymentStatus == PaymentStatus.Paid && inst.IsLate)
                {
                    inst.IsLate = false;
                    instChanged = true;
                }

                if (instChanged)
                {
                    await _installmentRepository.UpdateAsync(inst.Id, inst);
                    updatedInstallmentsCount++;
                }
            }

            bool isOverdue = loan.LoanInstallments.Any(i => i.IsLate);
            var newStatus = isOverdue ? ClientPaymentStatus.Defaulted : ClientPaymentStatus.UpToDate;

            if (loan.ClientPaymentStatus != newStatus)
            {
                loan.ClientPaymentStatus = newStatus;
                loanChanged = true;
            }

            if (loanChanged)
            {
                loan.LoanInstallments = new List<ABP.Core.Domain.Entities.LoanInstallment>();
                await _loanRepository.UpdateAsync(loan.Id, loan);
                updatedLoansCount++;
            }

            if (newlyLateInstallment)
            {
                var user = await _userManager.FindByIdAsync(loan.ClientId);
                if (user != null && user.EmailConfirmed && !string.IsNullOrWhiteSpace(user.Email))
                {
                    emailNotifications.Add((user.Email, $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#dc2626,#ef4444);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#9888;&#65039; Cuota Atrasada</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{user.Name}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">Le informamos que una o m&#225;s cuotas de su pr&#233;stamo han pasado a estado de <strong>atraso</strong>.</p>
<div style="background:#fee2e2;border-left:4px solid #dc2626;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#991b1b;font-size:13px;margin:0;">Por favor, realice su pago a la mayor brevedad posible para evitar cargos adicionales o paso a mora.</p>
</div>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
"""));
                }
            }
        }

        foreach (var notification in emailNotifications)
        {
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = notification.Email,
                Subject = "Aviso de Cuota Atrasada - Artemis Banking Pro",
                HtmlBody = notification.Message
            });
        }

        _logger.LogInformation("Overdue loans update completed. Loans updated: {LoansCount}, Installments updated: {InstallmentsCount}. Sent {EmailsCount} email notifications.", updatedLoansCount, updatedInstallmentsCount, emailNotifications.Count);
    }
}