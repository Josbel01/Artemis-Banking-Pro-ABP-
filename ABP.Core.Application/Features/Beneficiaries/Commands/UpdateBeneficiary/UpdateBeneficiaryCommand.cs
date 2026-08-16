using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Beneficiaries.Commands.UpdateBeneficiary 
{ 
    public class UpdateBeneficiaryCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;
    } 

    public class UpdateBeneficiaryCommandHandler : IRequestHandler<UpdateBeneficiaryCommand, Unit>
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly IBaseAccountService _accountService;

        public UpdateBeneficiaryCommandHandler(
            IBeneficiaryRepository beneficiaryRepository,
            ISavingAccountRepository savingAccountRepository,
            IBaseAccountService accountService)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _savingAccountRepository = savingAccountRepository;
            _accountService = accountService;
        }

        public async Task<Unit> Handle(UpdateBeneficiaryCommand command, CancellationToken cancellationToken)
        {
            var beneficiary = await _beneficiaryRepository.GetByIdAsync(command.Id);
            
            if (beneficiary == null)
            {
                throw new Exception("El beneficiario no existe.");
            }

            if (beneficiary.ClientId != command.ClientId)
            {
                throw new Exception("No tienes permisos para modificar este beneficiario.");
            }

            // Si intenta cambiar a la misma cuenta que ya tiene, no hacer nada.
            if (beneficiary.BeneficiaryAccountNumber == command.BeneficiaryAccountNumber)
            {
                return Unit.Value;
            }

            var destinationAccount = await _savingAccountRepository.GetByAccountNumberAsync(command.BeneficiaryAccountNumber);

            if (destinationAccount == null)
            {
                throw new Exception("La nueva cuenta de destino no existe.");
            }

            if (destinationAccount.ClientId == command.ClientId)
            {
                throw new Exception("No puedes agregarte a ti mismo como beneficiario.");
            }

            var existingBeneficiary = await _beneficiaryRepository.GetByAccountAndClientIdAsync(command.BeneficiaryAccountNumber, command.ClientId);
            if (existingBeneficiary != null)
            {
                throw new Exception("Ya tienes esta nueva cuenta en tu lista de beneficiarios.");
            }

            var accountOwner = await _accountService.GetUserById(destinationAccount.ClientId);
            if (accountOwner == null)
            {
                throw new Exception("No se pudo obtener la información del dueño de la cuenta.");
            }

            beneficiary.BeneficiaryAccountNumber = command.BeneficiaryAccountNumber;
            beneficiary.BeneficiaryName = accountOwner.FirstName;
            beneficiary.BeneficiaryLastName = accountOwner.LastName;

            await _beneficiaryRepository.UpdateAsync(beneficiary.Id, beneficiary);

            return Unit.Value;
        }
    }
}
