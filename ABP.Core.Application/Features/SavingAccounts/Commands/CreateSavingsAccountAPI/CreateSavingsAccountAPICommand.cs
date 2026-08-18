using MediatR;

namespace ABP.Core.Application.Features.SavingAccounts.Commands.CreateSavingsAccountAPI
{
    public class CreateSavingsAccountAPICommand : IRequest<object>
    {
        public string ClientId { get; set; } = string.Empty;
        public decimal InitialBalance { get; set; }
    }
}
