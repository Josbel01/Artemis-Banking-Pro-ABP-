using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.CancelSavingAccount
{
    public class CancelSavingAccountCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }

    public class CancelSavingAccountCommandHandler : IRequestHandler<CancelSavingAccountCommand, Unit>
    {
        private readonly ISavingAccountRepository _repository;

        public CancelSavingAccountCommandHandler(ISavingAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(CancelSavingAccountCommand command, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(command.Id);
            if (entity == null) throw new Exception("SavingAccount not found");

            if (entity.AccountType == SavingAccountType.Main)
                throw new Exception("Cannot cancel a principal saving account");

            entity.Status = SavingAccountStatus.Cancelled;

            await _repository.UpdateAsync(command.Id, entity);
            return Unit.Value;
        }
    }
}
