using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Beneficiaries.Commands.DeleteBeneficiary 
{ 
    public class DeleteBeneficiaryCommand : IRequest<Unit>
    {
        public required int Id { get; set; } 
    }

    public class DeleteBeneficiaryCommandHandler : IRequestHandler<DeleteBeneficiaryCommand, Unit>
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        public DeleteBeneficiaryCommandHandler(IBeneficiaryRepository beneficiaryRepository)
        {
            _beneficiaryRepository = beneficiaryRepository;
        }
        public async Task<Unit> Handle(DeleteBeneficiaryCommand command, CancellationToken cancellationToken)
        {
            Beneficiary? beneficiary = await _beneficiaryRepository.GetByIdAsync(command.Id);
            if (beneficiary == null) 
                throw new ArgumentException("Beneficiary not found with this id");

            await _beneficiaryRepository.DeleteAsync(command.Id);

            return Unit.Value;
        }
    }
}
