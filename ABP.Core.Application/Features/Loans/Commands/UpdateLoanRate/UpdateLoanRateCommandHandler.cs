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

            // Recalculate only future unpaid installments via service
            await _loanService.RecalculateFutureInstallmentsAsync(id, request.AnnualInterestRate);

            // Send email using centralized template
            try
            {
                var client = await _accountService.GetUserById(loan.ClientId);
                if (client != null)
                {
                    var allInstallments = await _loanInstallmentService.GetAllByLoanIdAsync(id);
                    var nextPending = allInstallments?
                        .Where(i => i.PaymentStatus != PaymentStatus.Paid && i.DueDate.Date > DateTime.Now.Date)
                        .OrderBy(i => i.InstallmentNumber)
                        .FirstOrDefault();

                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = client.Email,
                        Subject = $"Actualización de tasa de interés - Préstamo #{loan.LoanNumber}",
                        HtmlBody = EmailTemplates.LoanRateUpdated(
                            client.FirstName, loan.LoanNumber, request.AnnualInterestRate,
                            nextPending?.InstallmentAmount ?? 0,
                            nextPending?.DueDate.ToString("dd/MM/yyyy") ?? "N/A")
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
