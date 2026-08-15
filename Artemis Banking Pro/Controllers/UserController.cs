using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.User;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBankingPro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IAccountServiceWebApp _accountService;
        private readonly ISavingAccountService _savingAccountService;
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;

        public UserController(
            IAccountServiceWebApp accountService, 
            ISavingAccountService savingAccountService,
            ITransactionService transactionService,
            IMapper mapper)
        {
            _accountService = accountService;
            _savingAccountService = savingAccountService;
            _transactionService = transactionService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _accountService.GetAllUser(null);
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(users);
            return View(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _accountService.GetUserById(id);
            if (user != null)
            {
                var saveUserDto = new ABP.Core.Application.Dtos.User.SaveUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    DNI = user.DNI ?? "",
                    Role = user.Roles?.FirstOrDefault() ?? "",
                    Password = "",
                    ConfirmPassword = "",
                    IsActive = !user.IsActive
                };
                await _accountService.EditUser(saveUserDto, null, false, false);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var saveUserDto = new ABP.Core.Application.Dtos.User.SaveUserDto
            {
                Id = null,
                FirstName = viewModel.Name,
                LastName = viewModel.LastName,
                UserName = viewModel.UserName,
                Email = viewModel.Email,
                DNI = viewModel.Identification,
                Role = viewModel.Role,
                Password = viewModel.Password,
                ConfirmPassword = viewModel.ConfirmPassword
            };

            var response = await _accountService.RegisterUser(saveUserDto, null, false);
            
            if (response.HasError)
            {
                foreach(var error in response.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(viewModel);
            }

            if (saveUserDto.Role == "Client")
            {
                var rnd = new Random();
                string accountNumber = rnd.Next(100000000, 999999999).ToString();
                
                var newAccount = new ABP.Core.Application.Dtos.SavingAccounts.SavingAccountDto
                {
                    Id = 0,
                    ClientId = response.Id,
                    AccountNumber = accountNumber,
                    Balance = viewModel.InitialAmount,
                    AccountType = ABP.Core.Domain.Common.Enums.SavingAccountType.Main,
                    Status = ABP.Core.Domain.Common.Enums.SavingAccountStatus.Active
                };
                
                var createdAccount = await _savingAccountService.AddAsync(newAccount);

                if (viewModel.InitialAmount > 0)
                {
                    await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                    {
                        SavingAccountId = createdAccount.Id,
                        Amount = viewModel.InitialAmount,
                        Type = ABP.Core.Domain.Common.Enums.TransactionType.Credit,
                        TransactionDate = DateTime.Now,
                        Origin = "Apertura de Cuenta",
                        Beneficiary = accountNumber,
                        Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
                    });
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _accountService.GetUserById(id);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = new UpdateUserViewModel
            {
                Id = user.Id,
                Name = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Identification = user.DNI ?? "",
                Role = user.Roles?.FirstOrDefault() ?? "",
                IsActive = user.IsActive,
                InitialAmount = 0
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var saveUserDto = new ABP.Core.Application.Dtos.User.SaveUserDto
            {
                Id = viewModel.Id,
                FirstName = viewModel.Name,
                LastName = viewModel.LastName,
                UserName = viewModel.UserName,
                Email = viewModel.Email,
                DNI = viewModel.Identification,
                Role = viewModel.Role ?? "",
                Password = viewModel.Password ?? "",
                ConfirmPassword = viewModel.ConfirmPassword ?? "",
                IsActive = viewModel.IsActive
            };

            var response = await _accountService.EditUser(saveUserDto, null, false, false);
            
            if (response.HasError)
            {
                foreach(var error in response.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(viewModel);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
