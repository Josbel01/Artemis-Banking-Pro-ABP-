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
        private readonly IMapper _mapper;

        public UserController(IAccountServiceWebApp accountService, IMapper mapper)
        {
            _accountService = accountService;
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

            // Note: If initial amount needs to be added, we need a service for it, 
            // but for now creating the identity user is done here.

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
