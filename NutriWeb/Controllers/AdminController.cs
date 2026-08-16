using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriWeb.Data;
using NutriWeb.Models;

namespace NutriWeb.Controllers
{
    public class AdminUserSubscriptionViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<UserSubscription> ActiveSubscriptions { get; set; } = new();
    }

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            IWebHostEnvironment environment,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _environment = environment;
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin/VideoConstructor
        [HttpGet]
        public async Task<IActionResult> VideoConstructor(string? searchEmail = null)
        {
            ViewBag.Recipes = RecipeController.Recipes;

            var usersQuery = _userManager.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchEmail))
            {
                usersQuery = usersQuery.Where(u => u.Email!.Contains(searchEmail) || u.UserName!.Contains(searchEmail));
                ViewBag.SearchEmail = searchEmail;
            }

            var users = await usersQuery.Take(25).ToListAsync();
            var userSubscriptionsList = new List<AdminUserSubscriptionViewModel>();

            foreach (var user in users)
            {
                var subs = await _context.UserSubscriptions
                    .Where(s => s.UserId == user.Id && s.ExpirationDate >= DateTime.UtcNow)
                    .ToListAsync();

                userSubscriptionsList.Add(new AdminUserSubscriptionViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? "Неуказан",
                    ActiveSubscriptions = subs
                });
            }

            ViewBag.UserSubscriptions = userSubscriptionsList;
            return View();
        }

        // POST: /Admin/GrantSubscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantSubscription(string userEmail, int priceGroupId, int days = 30)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                TempData["Error"] = "Укажите логин (Email) пользователя!";
                return RedirectToAction("VideoConstructor");
            }

            var user = await _userManager.FindByEmailAsync(userEmail.Trim())
                       ?? await _userManager.FindByNameAsync(userEmail.Trim());

            if (user == null)
            {
                TempData["Error"] = $"Пользователь с логином «{userEmail}» не найден!";
                return RedirectToAction("VideoConstructor");
            }

            // ГАРАНТИРУЕМ НАЛИЧИЕ ГРУППЫ В ТАБЛИЦЕ PriceGroups ДЛЯ ИЗБЕЖАНИЯ FOREIGN KEY FAIL
            var groupExists = await _context.PriceGroups.AnyAsync(g => g.Id == priceGroupId);
            if (!groupExists)
            {
                var groupNames = new Dictionary<int, (string Name, string Desc, decimal Price)>
                {
                    { 1, ("Чистое питание", "Базовые блюда, супы, горячее и соусы", 450) },
                    { 2, ("Живое тесто & Сладкое", "Безглютеновый хлеб, выпечка, десерты", 650) },
                    { 3, ("Суперфуды & Смузи", "Детокс-напитки, витаминные боулы и эликсиры", 350) }
                };

                if (groupNames.TryGetValue(priceGroupId, out var info))
                {
                    _context.PriceGroups.Add(new PriceGroup
                    {
                        Id = priceGroupId,
                        Name = info.Name,
                        Description = info.Desc,
                        PriceUah = info.Price
                    });
                    await _context.SaveChangesAsync();
                }
            }

            var existingSub = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == user.Id && s.PriceGroupId == priceGroupId);

            if (existingSub != null)
            {
                existingSub.ExpirationDate = DateTime.UtcNow.AddDays(days);
                _context.UserSubscriptions.Update(existingSub);
            }
            else
            {
                var newSub = new UserSubscription
                {
                    UserId = user.Id,
                    PriceGroupId = priceGroupId,
                    StartDate = DateTime.UtcNow,
                    ExpirationDate = DateTime.UtcNow.AddDays(days)
                };
                await _context.UserSubscriptions.AddAsync(newSub);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Подписка на категорию #{priceGroupId} активирована/восстановлена для {user.Email} на {days} дней!";
            return RedirectToAction("VideoConstructor", new { searchEmail = userEmail });
        }

        // POST: /Admin/RevokeSubscription
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeSubscription(string userEmail, int priceGroupId)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                TempData["Error"] = "Укажите логин (Email) пользователя!";
                return RedirectToAction("VideoConstructor");
            }

            var user = await _userManager.FindByEmailAsync(userEmail.Trim())
                       ?? await _userManager.FindByNameAsync(userEmail.Trim());

            if (user == null)
            {
                TempData["Error"] = $"Пользователь с логином «{userEmail}» не найден!";
                return RedirectToAction("VideoConstructor");
            }

            int deletedRows = await _context.UserSubscriptions
                .Where(s => s.UserId == user.Id && s.PriceGroupId == priceGroupId)
                .ExecuteDeleteAsync();

            if (deletedRows > 0)
            {
                TempData["Success"] = $"Подписка на категорию #{priceGroupId} аннулирована для пользователя {user.Email}!";
            }
            else
            {
                TempData["Error"] = $"У пользователя {user.Email} нет активной подписки на категорию #{priceGroupId}.";
            }

            return RedirectToAction("VideoConstructor", new { searchEmail = userEmail });
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["Error"] = "Идентификатор пользователя не указан!";
                return RedirectToAction("VideoConstructor");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == userId)
            {
                TempData["Error"] = "Запрещено удалять собственный аккаунт администратора!";
                return RedirectToAction("VideoConstructor");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Пользователь не найден!";
                return RedirectToAction("VideoConstructor");
            }

            await _context.UserSubscriptions
                .Where(s => s.UserId == user.Id)
                .ExecuteDeleteAsync();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"Аккаунт «{user.Email}» полностью удален из системы!";
            }
            else
            {
                TempData["Error"] = $"Ошибка при удалении: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction("VideoConstructor");
        }

        // POST: /Admin/UploadVideo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadVideo(int recipeId, IFormFile? videoFile)
        {
            var recipe = RecipeController.Recipes.FirstOrDefault(r => r.Id == recipeId);
            if (recipe == null)
            {
                TempData["Error"] = "Рецепт не найден!";
                return RedirectToAction("VideoConstructor");
            }

            if (videoFile == null || videoFile.Length == 0)
            {
                TempData["Error"] = "Выберите видеофайл для загрузки!";
                return RedirectToAction("VideoConstructor");
            }

            var allowedExtensions = new[] { ".mp4", ".webm", ".ogg", ".mov" };
            var extension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Недопустимый формат! Разрешены MP4, WEBM, MOV.";
                return RedirectToAction("VideoConstructor");
            }

            string videoDir = Path.Combine(_environment.WebRootPath, "Video");
            if (!Directory.Exists(videoDir))
            {
                Directory.CreateDirectory(videoDir);
            }

            if (!string.IsNullOrEmpty(recipe.VideoUrl))
            {
                string oldPath = Path.Combine(_environment.WebRootPath, recipe.VideoUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            string fileName = $"recipe_{recipeId}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
            string fullPath = Path.Combine(videoDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            recipe.VideoUrl = $"/Video/{fileName}";

            TempData["Success"] = $"Видео успешно прикреплено к рецепту «{recipe.Title}»!";
            return RedirectToAction("VideoConstructor");
        }

        // POST: /Admin/DeleteVideo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVideo(int recipeId)
        {
            var recipe = RecipeController.Recipes.FirstOrDefault(r => r.Id == recipeId);
            if (recipe == null)
            {
                TempData["Error"] = "Рецепт не найден!";
                return RedirectToAction("VideoConstructor");
            }

            if (!string.IsNullOrEmpty(recipe.VideoUrl))
            {
                string fullPath = Path.Combine(_environment.WebRootPath, recipe.VideoUrl.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                recipe.VideoUrl = null;
                TempData["Success"] = $"Видео успешно удалено из рецепта «{recipe.Title}»!";
            }
            else
            {
                TempData["Error"] = "У данного рецепта нет прикрепленного видео.";
            }

            return RedirectToAction("VideoConstructor");
        }
    }
}