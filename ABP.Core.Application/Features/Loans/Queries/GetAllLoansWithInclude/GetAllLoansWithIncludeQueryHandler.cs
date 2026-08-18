using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Loans.Queries.GetAllLoansWithInclude
{
    public class GetAllLoansWithIncludeQueryHandler : IRequestHandler<GetAllLoansWithIncludeQuery, object>
    {
        private readonly ILoanService _loanService;
        private readonly IAccountServiceWebApi _userManager;

        public GetAllLoansWithIncludeQueryHandler(ILoanService loanService, IAccountServiceWebApi userManager)
        {
            _loanService = loanService;
            _userManager = userManager;
        }

        public async Task<object> Handle(GetAllLoansWithIncludeQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanService.GetAllWithIncludeAsync(new List<string> { "LoanInstallments" });

            loans = loans.OrderByDescending(l => l.Id).ToList();

            var allUsers = await _userManager.GetAllUser(null);
            var paged = loans.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new
            {
                page = request.PageNumber,
                pageSize = request.PageSize,
                totalRecords = loans.Count,
                totalPages = loans.Count == 0 ? 1 : (int)Math.Ceiling(loans.Count / (double)request.PageSize),
                data = paged.Select(l =>
                {
                    var client = allUsers.FirstOrDefault(u => u.Id == l.ClientId);
                    return new
                    {
                        id = l.Id.ToString(),
                        loanNumber = l.LoanNumber,
                        clientId = l.ClientId,
                        clientFullName = client != null ? $"{client.FirstName} {client.LastName}" : "",
                        capitalAmount = l.AmountApproved,
                        pendingAmount = l.AmountPending,
                        annualInterestRate = l.AnnualInterestRate,
                        termInMonths = l.TermInMonths,
                        status = l.Status == LoanStatus.Active ? "Activo" : "Completado",
                        installments = l.LoanInstallments.Select(i => new
                        {
                            id = i.Id,
                            installmentNumber = i.InstallmentNumber,
                            installmentAmount = i.InstallmentAmount,
                            capitalAmount = i.CapitalAmount,
                            interestAmount = i.InterestAmount,
                            pendingAmount = i.PendingAmount,
                            dueDate = i.DueDate,
                            paymentStatus = i.PaymentStatus.ToString(),
                            isLate = i.IsLate
                        })
                    };
                })
            };
        }
    }
}
