using MediatR;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Helpers;
using ABP.Core.Domain.Common.Enums;
using System.Linq;
using System;
using ABP.Core.Application.Dtos.Email;

namespace ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommandHandler : IRequestHandler<UpdateLoanRateCommand, bool>
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly IEmailService _emailService;
        private readonly IBaseAccountService _accountService;

        public UpdateLoanRateCommandHandler(
            ILoanService loanService,
            ILoanInstallmentService loanInstallmentService,
            IEmailService emailService,
            IBaseAccountService accountService)
        {
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
            _emailService = emailService;
            _accountService = accountService;
        }

        public async Task<bool> Handle(UpdateLoanRateCommand request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.Id, out int id)) return false;

            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null) return false;

            var pendingInstallments = (await _loanInstallmentService.GetAllByLoanIdAsync(id))
                .Where(installment => installment.PaymentStatus == PaymentStatus.Pending && installment.PendingAmount > 0 && installment.DueDate.Date > DateTime.Now.Date)
                .OrderBy(installment => installment.InstallmentNumber)
                .ToList();

            if (pendingInstallments.Count > 0)
            {
                var remainingPrincipal = pendingInstallments.Sum(installment => installment.CapitalAmount);
                var recalculated = LoanAmortizationCalculator.GenerateAmortizationSchedule(
                    remainingPrincipal, request.AnnualInterestRate, pendingInstallments.Count, DateTime.UtcNow);
                for (var index = 0; index < pendingInstallments.Count; index++)
                {
                    var current = pendingInstallments[index];
                    var updated = recalculated[index];
                    current.InstallmentAmount = updated.InstallmentAmount;
                    current.InterestAmount = updated.InterestAmount;
                    current.CapitalAmount = updated.CapitalAmount;
                    current.PendingAmount = updated.PendingAmount;
                    await _loanInstallmentService.UpdateAsync(current, current.Id);
                }
                loan.AmountPending = pendingInstallments.Sum(installment => installment.PendingAmount);
            }

            loan.AnnualInterestRate = request.AnnualInterestRate;
            await _loanService.UpdateAsync(loan, id);

            // Send email to client with next pending installment info
            try
            {
                var client = await _accountService.GetUserById(loan.ClientId);
                if (client != null)
                {
                    var nextPending = pendingInstallments.FirstOrDefault();
                    var nextDueDate = nextPending != null ? nextPending.DueDate.ToString("dd/MM/yyyy") : "N/A";
                    var nextAmount = nextPending != null ? nextPending.InstallmentAmount.ToString("N2") : "N/A";

                    var emailBody = $"""
<!DOCTYPE html>
<html><head><meta charset="utf-8"></head>
<body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;">
<div style="max-width:520px;margin:30px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">
<div style="background:linear-gradient(135deg,#7c3aed,#a78bfa);padding:28px 30px;text-align:center;">
<h1 style="color:#fff;margin:0;font-size:20px;">&#128200; Actualizaci&#243;n de Tasa de Inter&#233;s</h1>
</div>
<div style="padding:30px;">
<p style="color:#334155;font-size:15px;margin:0 0 18px;">Hola <strong>{client.FirstName}</strong>,</p>
<p style="color:#334155;font-size:15px;margin:0 0 24px;">La tasa de inter&#233;s de su pr&#233;stamo <strong>#{loan.LoanNumber}</strong> ha sido actualizada.</p>
<table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Nueva tasa anual</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#7c3aed;border-radius:0 6px 6px 0;">{request.AnnualInterestRate}%</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Nuevo valor de pr&#243;xima cuota</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#16a34a;">RD${nextAmount}</td></tr>
<tr><td style="padding:10px 14px;background:#f8fafc;color:#64748b;font-size:13px;font-weight:600;border-radius:6px 0 0 6px;">Fecha de vencimiento pr&#243;xima cuota</td><td style="padding:10px 14px;background:#f8fafc;font-size:15px;font-weight:700;color:#0b1f3a;border-radius:0 6px 6px 0;">{nextDueDate}</td></tr>
<tr><td style="padding:10px 14px;color:#64748b;font-size:13px;font-weight:600;">Capital pendiente</td><td style="padding:10px 14px;font-size:15px;font-weight:700;color:#0b1f3a;">RD${loan.AmountPending:N2}</td></tr>
</table>
<div style="background:#fef9c3;border-left:4px solid #ca8a04;padding:12px 16px;border-radius:0 6px 6px 0;margin-bottom:20px;">
<p style="color:#854d0e;font-size:13px;margin:0;">&#9888;&#65039; Solo las cuotas futuras pendientes han sido recalculadas.</p>
</div>
</div>
<div style="background:#f8fafc;padding:18px 30px;text-align:center;border-top:1px solid #e2e8f0;">
<p style="color:#94a3b8;font-size:11px;margin:0;">Artemis Banking Pro &mdash; Plataforma de Banca Digital ITLA</p>
</div>
</div>
</body></html>
""";

                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = client.Email,
                        Subject = $"Actualización de tasa de interés - Préstamo #{loan.LoanNumber}",
                        HtmlBody = emailBody
                    });
                }
            }
            catch
            {
                // Email failure should not block the rate update
            }

            return true;
        }
    }
}
