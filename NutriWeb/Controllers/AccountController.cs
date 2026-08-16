using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriWeb.Data;
using NutriWeb.Models;

namespace NutriWeb.Controllers
{
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<ActiveSubItem> ActiveSubscriptions { get; set; } = new();
    }

    public class ActiveSubItem
    {
        public int PriceGroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int MonthsPeriod { get; set; } = 1;
    }

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        private static readonly Dictionary<int, string> GroupNames = new()
        {
            { 1, "Чистое питание" },
            { 2, "Живое тесто & Сладкое" },
            { 3, "Суперфуды & Смузи" }
        };

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Неверная попытка входа. Проверьте Email и пароль.");
            }

            return View(model);
        }

        // GET: /Account/Profile (ЛИЧНЫЙ КАБИНЕТ)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var activeSubs = await _context.UserSubscriptions
                .Where(s => s.UserId == user.Id && s.ExpirationDate >= DateTime.UtcNow)
                .ToListAsync();

            var subItems = activeSubs.Select(s => new ActiveSubItem
            {
                PriceGroupId = s.PriceGroupId,
                GroupName = GroupNames.ContainsKey(s.PriceGroupId)
                    ? GroupNames[s.PriceGroupId]
                    : $"Категория #{s.PriceGroupId}",
                ExpirationDate = s.ExpirationDate,
                MonthsPeriod = s.MonthsPeriod
            }).ToList();

            var viewModel = new UserProfileViewModel
            {
                User = user,
                ActiveSubscriptions = subItems
            };

            ViewBag.GroupNames = GroupNames;
            ViewBag.Subscriptions = activeSubs;

            return View(viewModel);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}