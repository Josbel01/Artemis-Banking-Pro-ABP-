using ABP.Core.Application.Interfaces;
using ABP.Core.Application.Dtos.Loans;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.Loans.Commands.CreateLoan
{
    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, object>
    {
        private readonly ILoanService _loanService;

        public CreateLoanCommandHandler(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public async Task<object> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
        {
            var loan = new LoanDto
            {
                Id = 0,
                ClientId = request.ClientId,
                AmountApproved = request.CapitalAmount,
                TermInMonths = request.TermInMonths,
                AnnualInterestRate = request.AnnualInterestRate,
                Status = LoanStatus.Active,
                LoanNumber = new Random().Next(100000000, 999999999).ToString(),
                TotalInstallments = request.TermInMonths
            };

            var created = await _loanService.AddAsync(loan);

            return new
            {
                id = created.Id.ToString(),
                loanNumber = created.LoanNumber,
                clientId = created.ClientId,
                capitalAmount = created.AmountApproved,
                termInMonths = created.TermInMonths,
                annualInterestRate = created.AnnualInterestRate,
                monthlyInstallment = 0, 
                totalAmountToPay = 0,
                status = "Activo"
            };
        }
    }
}
