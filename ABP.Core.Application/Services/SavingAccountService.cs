using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class SavingAccountService : GenericService<SavingAccount, SavingAccountDto>, ISavingAccountService
    {
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly IMapper _mapper;

        public SavingAccountService(ISavingAccountRepository savingAccountRepository, IMapper mapper) 
            : base(savingAccountRepository, mapper)
        {
            _savingAccountRepository = savingAccountRepository;
            _mapper = mapper;
        }

        public async Task<List<SavingAccountDto>> GetAllByClientIdAsync(string clientId)
        {
            var accounts = await _savingAccountRepository.GetAllListAsync();
            var clientAccounts = accounts.Where(a => a.ClientId == clientId).ToList();
            return _mapper.Map<List<SavingAccountDto>>(clientAccounts);
        }

        public async Task<SavingAccountDto?> GetByAccountNumberAsync(string accountNumber)
        {
            var accounts = await _savingAccountRepository.GetAllListAsync();
            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            return account == null ? null : _mapper.Map<SavingAccountDto>(account);
        }
    }
}
