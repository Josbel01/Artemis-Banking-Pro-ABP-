using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Loans.Queries.GetAllLoans
{
    public class GetAllLoansQueryHandler : IRequestHandler<GetAllLoansQuery, object>
    {
        private readonly ILoanService _loanService;
        private readonly IBaseAccountService _userManager;

        public GetAllLoansQueryHandler(ILoanService loanService, IBaseAccountService userManager)
        {
            _loanService = loanService;
            _userManager = userManager;
        }

        public async Task<object> Handle(GetAllLoansQuery request, CancellationToken cancellationToken)
        {
            var loans = await _loanService.GetAllAsync();

            if (!string.IsNullOrEmpty(request.Identification))
            {
                var allUsers = await _userManager.GetAllUser(null);
                var user = allUsers.FirstOrDefault(u => u.DNI == request.Identification);
                if (user != null)
                {
                    loans = loans.Where(l => l.ClientId == user.Id).ToList();
                }
                else
                {
                    loans = new List<ABP.Core.Application.Dtos.Loans.LoanDto>();
                }
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (request.Status.ToLower() == "activos")
                {
                    loans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
                }
                else if (request.Status.ToLower() == "completados")
                {
                    loans = loans.Where(l => l.Status == LoanStatus.Completed).ToList();
                }
            }

            loans = loans.OrderByDescending(l => l.Id).ToList();

            var allUsersForNames = await _userManager.GetAllUser(null);
            var paged = loans.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new
            {
                page = request.PageNumber,
                pageSize = request.PageSize,
                totalRecords = loans.Count,
                totalPages = loans.Count == 0 ? 1 : (int)Math.Ceiling(loans.Count / (double)request.PageSize),
                data = paged.Select(l =>
                {
                    var client = allUsersForNames.FirstOrDefault(u => u.Id == l.ClientId);
                    return new
                    {
                        id = l.Id.ToString(),
                        loanNumber = l.LoanNumber,
                        clientId = l.ClientId,
                        clientFullName = client != null ? $"{client.FirstName} {client.LastName}" : "",
                        capitalAmount = l.AmountApproved,
                        totalInstallments = l.TotalInstallments,
                        paidInstallments = l.PaidInstallments,
                        pendingAmount = l.AmountPending,
                        annualInterestRate = l.AnnualInterestRate,
                        termInMonths = l.TermInMonths,
                        status = l.Status == LoanStatus.Active ? "Activo" : "Completado",
                        clientPaymentStatus = l.ClientPaymentStatus switch
                        {
                            ClientPaymentStatus.UpToDate => "Al día",
                            ClientPaymentStatus.Late => "Atrasado",
                            ClientPaymentStatus.Defaulted => "En mora",
                            _ => l.ClientPaymentStatus.ToString()
                        }
                    };
                })
            };
        }
    }
}
