using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.LoanInstallments.Commands.CreateLoanInstallment
{
    public class CreateLoanInstallmentCommandHandler : IRequestHandler<CreateLoanInstallmentCommand, int>
    {
        private readonly ILoanInstallmentRepository _loanInstallmentRepository;
        private readonly ILogger<CreateLoanInstallmentCommandHandler> _logger;

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
                PaymentStatus = PaymentStatus.Pending,
                IsLate = false
            };

            var result = await _loanInstallmentRepository.AddAsync(entity);

            _logger.LogInformation("Loan installment creation result: {Result}", result != null ? "Success" : "Failure");

            if (result == null)
                throw new Exception("Error creating loan installment");

            return result.Id;
        }
    }
}
