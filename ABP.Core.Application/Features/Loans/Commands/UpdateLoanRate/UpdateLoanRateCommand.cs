using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Loans.Commands.UpdateLoanRate
{
    public class UpdateLoanRateCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public decimal NewAnnualInterestRate { get; set; }
    }

    public class UpdateLoanRateCommandHandler : IRequestHandler<UpdateLoanRateCommand, Unit>
    {
        private readonly ILoanRepository _repository;

        public UpdateLoanRateCommandHandler(ILoanRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateLoanRateCommand command, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(command.Id);
            if (entity == null) throw new Exception("Loan not found");

            entity.AnnualInterestRate = command.NewAnnualInterestRate;

            await _repository.UpdateAsync(command.Id, entity);
            return Unit.Value;
        }
    }
}
