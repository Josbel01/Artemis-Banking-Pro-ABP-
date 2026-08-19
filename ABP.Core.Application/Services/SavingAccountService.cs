using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class SavingAccountService : GenericService<SavingAccount, SavingAccountDto>, ISavingAccountService
    {
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SavingAccountService> _logger;

        public SavingAccountService(ISavingAccountRepository savingAccountRepository, IMapper mapper, ILoggerFactory loggerFactory) 
            : base(savingAccountRepository, mapper, loggerFactory.CreateLogger<GenericService<SavingAccount, SavingAccountDto>>())
        {
            _savingAccountRepository = savingAccountRepository;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<SavingAccountService>();
        }

        public async Task<List<SavingAccountDto>> GetAllByClientIdAsync(string clientId)
        {
            _logger.LogInformation("Retrieving all saving accounts for client ID: {ClientId}", clientId);
            var accounts = await _savingAccountRepository.GetAllListAsync();
            var clientAccounts = accounts.Where(a => a.ClientId == clientId).ToList();
            _logger.LogInformation("Found {Count} saving accounts for client ID: {ClientId}", clientAccounts.Count, clientId);
            return _mapper.Map<List<SavingAccountDto>>(clientAccounts);
        }

        public async Task<SavingAccountDto?> GetByAccountNumberAsync(string accountNumber)
        {
            _logger.LogInformation("Retrieving saving account by account number");
            var accounts = await _savingAccountRepository.GetAllListAsync();
            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            
            if (account == null)
            {
                _logger.LogWarning("Saving account not found");
                return null;
            }

            _logger.LogInformation("Saving account found");
            return _mapper.Map<SavingAccountDto>(account);
        }
    }
}
