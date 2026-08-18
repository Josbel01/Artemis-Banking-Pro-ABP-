using MediatR;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.DeleteSavingsAccountAPI
{
    public class DeleteSavingsAccountAPICommand : IRequest<bool>
    {
        public string AccountNumber { get; set; } = string.Empty;
    }
}
