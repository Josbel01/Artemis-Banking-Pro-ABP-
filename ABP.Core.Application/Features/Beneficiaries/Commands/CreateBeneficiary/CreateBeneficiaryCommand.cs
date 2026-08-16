using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.Beneficiaries.Commands.CreateBeneficiary 
{ 
    public class CreateBeneficiaryCommand : IRequest<int>
    {
        public string ClientId { get; set; } = string.Empty;
        public string BeneficiaryAccountNumber { get; set; } = string.Empty;
    } 

    public class CreateBeneficiaryCommandHandler : IRequestHandler<CreateBeneficiaryCommand, int>
    {
        private readonly IBeneficiaryRepository _beneficiaryRepository;
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly IBaseAccountService _accountService;

        public CreateBeneficiaryCommandHandler(
            IBeneficiaryRepository beneficiaryRepository,
            ISavingAccountRepository savingAccountRepository,
            IBaseAccountService accountService)
        {
            _beneficiaryRepository = beneficiaryRepository;
            _savingAccountRepository = savingAccountRepository;
            _accountService = accountService;
        }

        public async Task<int> Handle(CreateBeneficiaryCommand command, CancellationToken cancellationToken)
        {
            var destinationAccount = await _savingAccountRepository.GetByAccountNumberAsync(command.BeneficiaryAccountNumber);

            if (destinationAccount == null)
            {
                throw new Exception("La cuenta de destino no existe.");
            }

            if (destinationAccount.ClientId == command.ClientId)
            {
                throw new Exception("No puedes agregarte a ti mismo como beneficiario.");
            }

            var existingBeneficiary = await _beneficiaryRepository.GetByAccountAndClientIdAsync(command.BeneficiaryAccountNumber, command.ClientId);
            if (existingBeneficiary != null)
            {
                throw new Exception("Ya tienes a este usuario en tu lista de beneficiarios.");
            }

            var accountOwner = await _accountService.GetUserById(destinationAccount.ClientId);
            if (accountOwner == null)
            {
                throw new Exception("No se pudo obtener la información del dueño de la cuenta.");
            }

            var beneficiary = new Beneficiary
            {
                ClientId = command.ClientId,
                BeneficiaryAccountNumber = command.BeneficiaryAccountNumber,
                BeneficiaryName = accountOwner.FirstName,
                BeneficiaryLastName = accountOwner.LastName
            };

            await _beneficiaryRepository.AddAsync(beneficiary);

            return beneficiary.Id;
        }
    }
}