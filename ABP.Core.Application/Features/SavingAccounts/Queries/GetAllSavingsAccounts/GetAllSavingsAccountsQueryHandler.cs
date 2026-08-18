using ABP.Core.Application.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ABP.Core.Domain.Common.Enums;

namespace ABP.Core.Application.Features.SavingAccounts.Queries.GetAllSavingsAccounts
{
    public class GetAllSavingsAccountsQueryHandler : IRequestHandler<GetAllSavingsAccountsQuery, object>
    {
        private readonly ISavingAccountService _savingAccountService;
        private readonly IAccountServiceWebApi _userManager;

        public GetAllSavingsAccountsQueryHandler(ISavingAccountService savingAccountService, IAccountServiceWebApi userManager)
        {
            _savingAccountService = savingAccountService;
            _userManager = userManager;
        }

        public async Task<object> Handle(GetAllSavingsAccountsQuery request, CancellationToken cancellationToken)
        {
            var accounts = await _savingAccountService.GetAllAsync();

            if (!string.IsNullOrEmpty(request.Identification))
            {
                var allUsers = await _userManager.GetAllUser(null);
                var user = allUsers.FirstOrDefault(u => u.DNI == request.Identification);
                if (user != null)
                {
                    accounts = accounts.Where(a => a.ClientId == user.Id).ToList();
                }
                else
                {
                    accounts = new System.Collections.Generic.List<ABP.Core.Application.Dtos.SavingAccounts.SavingAccountDto>();
                }
            }

            if (request.Status.ToLower() == "activa")
            {
                accounts = accounts.Where(a => a.Status == SavingAccountStatus.Active).ToList();
            }
            else if (request.Status.ToLower() == "cancelada")
            {
                accounts = accounts.Where(a => a.Status == SavingAccountStatus.Cancelled).ToList();
            }

            if (request.Type.ToLower() == "principal")
            {
                accounts = accounts.Where(a => a.AccountType == SavingAccountType.Main).ToList();
            }
            else if (request.Type.ToLower() == "secundaria")
            {
                accounts = accounts.Where(a => a.AccountType == SavingAccountType.Secondary).ToList();
            }

            accounts = accounts.OrderByDescending(a => a.Id).ToList();

            var paged = accounts.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            return new
            {
                page = request.Page,
                pageSize = request.PageSize,
                totalRecords = accounts.Count,
                totalPages = accounts.Count == 0 ? 1 : (int)Math.Ceiling(accounts.Count / (double)request.PageSize),
                data = paged.Select(a => new {
                    id = a.Id.ToString(),
                    accountNumber = a.AccountNumber,
                    clientId = a.ClientId,
                    clientFullName = "", 
                    identification = "",
                    balance = a.Balance,
                    type = a.AccountType == SavingAccountType.Main ? "Principal" : "Secundaria",
                    status = a.Status == SavingAccountStatus.Active ? "Activa" : "Cancelada"
                })
            };
        }
    }
}

