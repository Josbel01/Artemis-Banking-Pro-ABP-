
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.LoanInstallments.Commands.CreateLoanInstallment
{
    /// <summary>
    /// Parameters required to create a new loan installment
    /// </summary>
    public class CreateLoanInstallmentCommand : IRequest<int>
    {
        [SwaggerParameter(Description = "The ID of the loan this installment belongs to")]
        public int LoanId { get; set; }

        [SwaggerParameter(Description = "The sequential number of this installment")]
        public int InstallmentNumber { get; set; }

        [SwaggerParameter(Description = "The due date for this installment")]
        public DateTime DueDate { get; set; }

        [SwaggerParameter(Description = "The total amount of the installment")]
        public decimal InstallmentAmount { get; set; }

        [SwaggerParameter(Description = "The interest portion of the installment")]
        public decimal InterestAmount { get; set; }

        [SwaggerParameter(Description = "The capital portion of the installment")]
        public decimal CapitalAmount { get; set; }
    }

    public class CreateLoanInstallmentCommandHandler : IRequestHandler<CreateLoanInstallmentCommand, int>
    {
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly ILogger _logger;

        public CreateLoanInstallmentCommandHandler(ILoanInstallmentRepository loanInstallmentRepository, ILoggerFactory loggerFactory)
        {
            _loanInstallmentRepository = loanInstallmentRepository;
            _logger = loggerFactory.CreateLogger<CreateLoanInstallmentCommandHandler>();
        }

        public async Task<int> Handle(CreateLoanInstallmentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating loan installment for LoanId: {LoanId}, Installment: {InstallmentNumber}", command.LoanId, command.InstallmentNumber);

            var entity = new LoanInstallment
            {
                LoanId = command.LoanId,
                InstallmentNumber = command.InstallmentNumber,
                DueDate = command.DueDate,
                InstallmentAmount = command.InstallmentAmount,
                InterestAmount = command.InterestAmount,
                CapitalAmount = command.CapitalAmount,
                PendingAmount = command.InstallmentAmount,
                PaymentStatus = ABP.Core.Domain.Common.Enums.PaymentStatus.Pending,
                IsLate = false
            };

            var result = await _loanInstallmentRepository.AddAsync(entity);

            _logger.LogInformation("Loan installment creation result: {Result}", result != null ? "Success" : "Failure");
            
            if (result == null)
            {
                throw new Exception("Error creating loan installment");
            }

            return result.Id;
        }
    }
}