using ABP.Core.Application.Dtos.SavingAccounts;
using ABP.Core.Application.Dtos.Transactions;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Common.Enums;
using ABP.Core.Domain.Entities;
using ABP.Core.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABP.Core.Application.Services
{
    public class SavingAccountService : GenericService<SavingAccount, SavingAccountDto>, ISavingAccountService
    {
        private readonly ISavingAccountRepository _savingAccountRepository;
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;
        private readonly ILogger<SavingAccountService> _logger;

        public SavingAccountService(ISavingAccountRepository savingAccountRepository, ITransactionService transactionService, IMapper mapper, ILoggerFactory loggerFactory) 
            : base(savingAccountRepository, mapper, loggerFactory.CreateLogger<GenericService<SavingAccount, SavingAccountDto>>())
        {
            _savingAccountRepository = savingAccountRepository;
            _transactionService = transactionService;
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

        public async Task CancelSecondaryAccountAsync(int accountId)
        {
            // Use entity directly to avoid tracking issues
            var entity = await _savingAccountRepository.GetByIdAsync(accountId);
            if (entity == null) return;

            if (entity.AccountType != SavingAccountType.Secondary || entity.Status != SavingAccountStatus.Active)
                return;

            if (entity.Balance > 0)
            {
                var allAccounts = await _savingAccountRepository.GetAllListAsync();
                var mainEntity = allAccounts.FirstOrDefault(a => a.ClientId == entity.ClientId && a.AccountType == SavingAccountType.Main && a.Status == SavingAccountStatus.Active);

                if (mainEntity != null)
                {
                    var transferAmount = entity.Balance;

                    // Debit from secondary account
                    await _transactionService.AddAsync(new TransactionDto
                    {
                        SavingAccountId = entity.Id,
                        Amount = transferAmount,
                        Type = TransactionType.Debit,
                        TransactionDate = DateTime.Now,
                        Origin = entity.AccountNumber,
                        Beneficiary = mainEntity.AccountNumber,
                        Status = TransactionStatus.Approved
                    });

                    // Credit to main account
                    mainEntity.Balance += transferAmount;
                    await _savingAccountRepository.UpdateAsync(mainEntity.Id, mainEntity);

                    await _transactionService.AddAsync(new TransactionDto
                    {
                        SavingAccountId = mainEntity.Id,
                        Amount = transferAmount,
                        Type = TransactionType.Credit,
                        TransactionDate = DateTime.Now,
                        Origin = entity.AccountNumber,
                        Beneficiary = mainEntity.AccountNumber,
                        Status = TransactionStatus.Approved
                    });
                }
            }

            // Set balance to 0 and cancel - using entity directly
            entity.Balance = 0;
            entity.Status = SavingAccountStatus.Cancelled;
            await _savingAccountRepository.UpdateAsync(entity.Id, entity);
        }
    }
}
