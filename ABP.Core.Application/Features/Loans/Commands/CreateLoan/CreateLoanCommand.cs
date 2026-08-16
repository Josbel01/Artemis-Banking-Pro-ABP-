
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Loans.Commands.CreateLoan
{
    /// <summary>
    /// Parameters required to create a new loan
    /// </summary>
    public class CreateLoanCommand : IRequest<int>
    {
        [SwaggerParameter(Description = "The generated loan number")]
        public string LoanNumber { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The Client ID assigned to the loan")]
        public string ClientId { get; set; } = string.Empty;

        [SwaggerParameter(Description = "The approved loan amount")]
        public decimal AmountApproved { get; set; }

        [SwaggerParameter(Description = "The annual interest rate")]
        public decimal AnnualInterestRate { get; set; }

        [SwaggerParameter(Description = "The loan term in months")]
        public int TermInMonths { get; set; }

        [SwaggerParameter(Description = "The user ID of the admin who assigned the loan")]
        public string AssignedByUserId { get; set; } = string.Empty;
    }

    public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, int>
    {
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger _logger;

        public CreateLoanCommandHandler(ILoanRepository loanRepository, ILoggerFactory loggerFactory)
        {
            _loanRepository = loanRepository;
            _logger = loggerFactory.CreateLogger<CreateLoanCommandHandler>();
        }

        public async Task<int> Handle(CreateLoanCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating loan for ClientId: {ClientId}, Amount: {AmountApproved}", command.ClientId, command.AmountApproved);

            var entity = new Loan
            {
                LoanNumber = command.LoanNumber,
                ClientId = command.ClientId,
                AmountApproved = command.AmountApproved,
                AmountPending = command.AmountApproved,
                AnnualInterestRate = command.AnnualInterestRate,
                TermInMonths = command.TermInMonths,
                AssignedByUserId = command.AssignedByUserId,
                TotalInstallments = command.TermInMonths,
                PaidInstallments = 0,
                ClientPaymentStatus = "Al Día",
                Status = ABP.Core.Domain.Common.Enums.LoanStatus.Active
            };

            var result = await _loanRepository.AddAsync(entity);

            _logger.LogInformation("Loan creation result: {Result}", result != null ? "Success" : "Failure");
            
            if (result == null)
            {
                throw new Exception("Error creating loan");
            }

            return result.Id;
        }
    }
}