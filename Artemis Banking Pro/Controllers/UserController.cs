using ABP.Core.Application.Interfaces;
using ABP.Core.Application.ViewModels.User;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        public async Task<IActionResult> Index(string filterRole = null, int page = 1)
        {
            int pageSize = 20;
            var users = await _accountService.GetAllUser(null);
            var allowedRoles = new List<string> {
                ABP.Core.Domain.Common.Enums.UserRoles.Admin.ToString(),
                ABP.Core.Domain.Common.Enums.UserRoles.Cashier.ToString(),
                ABP.Core.Domain.Common.Enums.UserRoles.Client.ToString()
            };
            var filteredUsers = users.Where(u => u.Roles != null && u.Roles.Any(r => allowedRoles.Contains(r))).ToList();

            if (!string.IsNullOrEmpty(filterRole))
            {
                filteredUsers = filteredUsers.Where(u => u.Roles != null && u.Roles.Contains(filterRole)).ToList();
            }

            filteredUsers = filteredUsers.OrderByDescending(u => u.CreatedAt).ToList();

            int totalRecords = filteredUsers.Count;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var pagedUsers = filteredUsers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModels = _mapper.Map<IEnumerable<UserViewModel>>(pagedUsers);

            ViewBag.CurrentFilter = filterRole;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "No puedes cambiar tu propio estado.";
                return RedirectToAction(nameof(Index));
            }

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
        [ValidateAntiForgeryToken]
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

            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _accountService.RegisterUser(saveUserDto, origin, false);

            if (response.HasError)
            {
                foreach (var error in response.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(viewModel);
            }

            // Create main saving account for Client users
            if (saveUserDto.Role == "Client")
            {
                await CreateMainAccountForClient(response.Id, viewModel.InitialAmount);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "No puedes editar tu propio perfil.";
                return RedirectToAction("Index");
            }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (viewModel.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "No puedes editar tu propio perfil.";
                return RedirectToAction("Index");
            }

            var existingUser = await _accountService.GetUserById(viewModel.Id);
            if (existingUser == null)
            {
                return RedirectToAction("Index");
            }

            var saveUserDto = new ABP.Core.Application.Dtos.User.SaveUserDto
            {
                Id = viewModel.Id,
                FirstName = viewModel.Name,
                LastName = viewModel.LastName,
                UserName = viewModel.UserName,
                Email = viewModel.Email,
                DNI = viewModel.Identification,
                Role = existingUser.Roles?.FirstOrDefault() ?? "",
                Password = viewModel.Password ?? "",
                ConfirmPassword = viewModel.ConfirmPassword ?? "",
                IsActive = viewModel.IsActive
            };

            var response = await _accountService.EditUser(saveUserDto, null, false, false);

            if (response.HasError)
            {
                foreach (var error in response.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(viewModel);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CreateMainAccountForClient(string clientId, decimal initialAmount)
        {
            var rnd = new Random();
            string accountNumber = rnd.Next(100000000, 999999999).ToString();

            var newAccount = new ABP.Core.Application.Dtos.SavingAccounts.SavingAccountDto
            {
                Id = 0,
                ClientId = clientId,
                AccountNumber = accountNumber,
                Balance = initialAmount,
                AccountType = ABP.Core.Domain.Common.Enums.SavingAccountType.Main,
                Status = ABP.Core.Domain.Common.Enums.SavingAccountStatus.Active
            };

            var createdAccount = await _savingAccountService.AddAsync(newAccount);

            if (initialAmount > 0)
            {
                await _transactionService.AddAsync(new ABP.Core.Application.Dtos.Transactions.TransactionDto
                {
                    SavingAccountId = createdAccount.Id,
                    Amount = initialAmount,
                    Type = ABP.Core.Domain.Common.Enums.TransactionType.Credit,
                    TransactionDate = DateTime.Now,
                    Origin = "Apertura de Cuenta",
                    Beneficiary = accountNumber,
                    Status = ABP.Core.Domain.Common.Enums.TransactionStatus.Approved
                });
            }
        }
    }
}
