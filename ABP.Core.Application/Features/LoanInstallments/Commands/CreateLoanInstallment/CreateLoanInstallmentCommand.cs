using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace ABP.Core.Application.Features.LoanInstallments.Commands.CreateLoanInstallment
{
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
}
