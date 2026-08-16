using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriWeb.Data;
using NutriWeb.Models;

namespace NutriWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            Dictionary<int, DateTime> activeSubscriptions = new();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // юбрнлюрхвеяйхи онкмши днярсо дкъ пнкх ADMIN
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        activeSubscriptions = new Dictionary<int, DateTime>
                        {
                            { 1, DateTime.UtcNow.AddYears(99) },
                            { 2, DateTime.UtcNow.AddYears(99) },
                            { 3, DateTime.UtcNow.AddYears(99) }
                        };
                    }
                    else
                    {
                        activeSubscriptions = await _context.UserSubscriptions
                            .Where(s => s.UserId == user.Id && s.ExpirationDate >= DateTime.UtcNow)
                            .GroupBy(s => s.PriceGroupId)
                            .ToDictionaryAsync(
                                g => g.Key,
                                g => g.Max(s => s.ExpirationDate)
                            );
                    }
                }
            }

            ViewBag.ActiveSubscriptions = activeSubscriptions;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}