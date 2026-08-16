using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriWeb.Data;
using NutriWeb.Models;

namespace NutriWeb.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Список тарифных групп для отображения формы и первичной инициализации в БД
        private static readonly List<PriceGroup> StaticPriceGroups = new()
        {
            new PriceGroup { Id = 1, Name = "Чистое питание", Description = "Базовые блюда, супы, горячее и соусы", PriceUah = 450 },
            new PriceGroup { Id = 2, Name = "Живое тесто & Сладкое", Description = "Безглютеновый хлеб, выпечка, десерты", PriceUah = 650 },
            new PriceGroup { Id = 3, Name = "Суперфуды & Смузи", Description = "Детокс-напитки, витаминные боулы и эликсиры", PriceUah = 350 }
        };

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Payment/Checkout?groupId=1&monthsPeriod=1
        [HttpGet]
        public IActionResult Checkout(int groupId, int monthsPeriod = 1)
        {
            // Корректировка значения периода при невалидных данных
            if (monthsPeriod != 1 && monthsPeriod != 6 && monthsPeriod != 12)
            {
                monthsPeriod = 1;
            }

            // Проверка авторизации вручную с корректным ReturnUrl
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Payment/Checkout?groupId={groupId}&monthsPeriod={monthsPeriod}" });
            }

            var priceGroup = StaticPriceGroups.FirstOrDefault(g => g.Id == groupId);
            if (priceGroup == null)
            {
                return RedirectToAction("PaidRecipes", "Recipe");
            }

            // Расчет итоговой стоимости с учетом скидки (25% на 6 месяцев, 50% на 12 месяцев)
            decimal discount = monthsPeriod switch
            {
                6 => 0.25m,
                12 => 0.50m,
                _ => 0.00m
            };

            decimal calculatedPrice = Math.Round((priceGroup.PriceUah * monthsPeriod) * (1 - discount));

            var model = new CheckoutViewModel
            {
                GroupId = priceGroup.Id,
                GroupName = priceGroup.Name,
                GroupDescription = priceGroup.Description,
                PriceUah = calculatedPrice,
                MonthsPeriod = monthsPeriod
            };

            return View(model);
        }

        // POST: /Payment/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                // ГАРАНТИЯ ЦЕЛОСТНОСТИ FOREIGN KEY: 
                // Проверяем, существует ли группа в БД. Если нет — добавляем её перед созданием подписки.
                var groupInDb = await _context.PriceGroups.FirstOrDefaultAsync(g => g.Id == model.GroupId);
                if (groupInDb == null)
                {
                    var groupToInsert = StaticPriceGroups.FirstOrDefault(g => g.Id == model.GroupId);
                    if (groupToInsert != null)
                    {
                        _context.PriceGroups.Add(new PriceGroup
                        {
                            Id = groupToInsert.Id,
                            Name = groupToInsert.Name,
                            Description = groupToInsert.Description,
                            PriceUah = groupToInsert.PriceUah
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                // Корректировка периода на случай передачи некорректных значений
                int monthsToAdd = model.MonthsPeriod switch
                {
                    6 => 6,
                    12 => 12,
                    _ => 1
                };

                // Проверяем, есть ли уже активная подписка на эту группу у пользователя
                var existingSubscription = await _context.UserSubscriptions
                    .FirstOrDefaultAsync(s => s.UserId == user.Id && s.PriceGroupId == model.GroupId && s.ExpirationDate > DateTime.UtcNow);

                if (existingSubscription != null)
                {
                    // Если подписка уже существует, продлеваем её с текущей даты окончания через метод модели
                    existingSubscription.MonthsPeriod = monthsToAdd;
                    existingSubscription.ExpirationDate = UserSubscription.CalculateExpirationDate(existingSubscription.ExpirationDate, monthsToAdd);
                    _context.UserSubscriptions.Update(existingSubscription);
                }
                else
                {
                    // Запись новой подписки на выбранный период в месяцах
                    var subscription = new UserSubscription
                    {
                        UserId = user.Id,
                        PriceGroupId = model.GroupId,
                        MonthsPeriod = monthsToAdd,
                        StartDate = DateTime.UtcNow,
                        ExpirationDate = UserSubscription.CalculateExpirationDate(null, monthsToAdd)
                    };

                    _context.UserSubscriptions.Add(subscription);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("PaidRecipes", "Recipe");
        }
    }
}