using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriWeb.Data;
using NutriWeb.Models;

namespace NutriWeb.Controllers
{
    public class PaidGroupViewModel
    {
        public PriceGroup Group { get; set; } = null!;
        public bool HasAccess { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public List<Recipe> Recipes { get; set; } = new();
    }

    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecipeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Коллекция из 11 БЕСПЛАТНЫХ рецептов
        private static readonly List<Recipe> FreeRecipesList = new()
        {
            new Recipe
            {
                Id = 101,
                Title = "Овсяноблин с авокадо и микрозеленью",
                Description = "Быстрый сытный завтрак без глютена и сахара для энергичного старта дня.",
                ImageUrl = "https://picsum.photos/id/1080/600/400",
                Ingredients = "• Овсяные хлопья без глютена — 40 г\n• Яйцо куриное — 1 шт\n• Немолоко (миндальное) — 30 мл\n• Авокадо — 0.5 шт\n• Микрозелень — 10 г\n\nКБЖУ на 100г: 165 ккал | Б: 5.8г | Ж: 8.2г | У: 17.1г",
                CookingSteps = "1. Измельчить хлопья, смешать с яйцом и молоком.\n2. Выпекать на антипригарной сковороде по 2 минуты с каждой стороны.\n3. Начинить дольками авокадо и посыпать микрозеленью."
            },
            new Recipe
            {
                Id = 102,
                Title = "Детокс-салат с огурцом и льном",
                Description = "Освежающий салат с клетчаткой для мягкого очищения организма.",
                ImageUrl = "https://picsum.photos/id/225/600/400",
                Ingredients = "• Огурцы свежие — 2 шт\n• Стебель сельдерея — 1 шт\n• Семена льна замоченные — 10 г\n• Оливковое масло — 10 мл\n• Лимонный сок — 5 мл\n\nКБЖУ на 100г: 68 ккал | Б: 1.2г | Ж: 5.1г | У: 4.3г",
                CookingSteps = "1. Нарезать огурцы и сельдерей тонкой соломкой.\n2. Добавить семена льна.\n3. Заправить оливковым маслом и лимонным соком."
            },
            new Recipe
            {
                Id = 103,
                Title = "Запеченная тыква с розмарином",
                Description = "Ароматный гарнир с медленными углеводами и бета-каротином.",
                ImageUrl = "https://picsum.photos/id/1062/600/400",
                Ingredients = "• Тыква свежая — 300 г\n• Розмарин свежий — 2 веточки\n• Масло оливковое — 10 мл\n• Семена тыквы — 15 г\n\nКБЖУ на 100г: 78 ккал | Б: 1.9г | Ж: 3.8г | У: 9.5г",
                CookingSteps = "1. Нарезать тыкву кубиками.\n2. Сбрызнуть маслом, выложить розмарин и семечки.\n3. Запекать при 190°C 25 минут."
            },
            new Recipe
            {
                Id = 104,
                Title = "Чиа-пудинг на кокосовом молоке",
                Description = "Легкий десерт с Омега-3 жирными кислотами и растительным белком.",
                ImageUrl = "https://picsum.photos/id/431/600/400",
                Ingredients = "• Семена чиа — 25 г\n• Кокосовое молоко — 150 мл\n• Малина свежая — 30 г\n• Стевия — по вкусу\n\nКБЖУ на 100г: 142 ккал | Б: 3.8г | Ж: 9.5г | У: 10.2г",
                CookingSteps = "1. Залить семена чиа кокосовым молоком со стевией.\n2. Оставить в холодильнике на 3 часа.\n3. Украсить свежей малиной перед подачей."
            },
            new Recipe
            {
                Id = 105,
                Title = "Крем-суп из цветной капусты",
                Description = "Нежный диетический суп с легкой текстурой и куркумой.",
                ImageUrl = "https://picsum.photos/id/493/600/400",
                Ingredients = "• Цветная капуста — 250 г\n• Куркума — 3 г\n• Сливки кокосовые — 50 мл\n• Овощной бульон — 200 мл\n\nКБЖУ на 100г: 58 ккал | Б: 2.1г | Ж: 3.2г | У: 5.4г",
                CookingSteps = "1. Отварить цветную капусту в бульоне 12 минут.\n2. Взбить блендером с куркумой и кокосовыми сливками.\n3. Подать горячим."
            },
            new Recipe
            {
                Id = 106,
                Title = "Зеленый боул с эдамаме и цукини",
                Description = "Витаминный биохакинг-микс для поддержки тонуса и иммунитета.",
                ImageUrl = "https://picsum.photos/id/326/600/400",
                Ingredients = "• Бобы эдамаме — 80 г\n• Цукини — 100 г\n• Шпинат — 30 г\n• Соус тахини — 15 г\n\nКБЖУ на 100г: 110 ккал | Б: 6.5г | Ж: 5.2г | У: 9.8г",
                CookingSteps = "1. Бланшировать эдамаме и нарезать цукини слайсами.\n2. Выложить на подушку из свежего шпината.\n3. Полить соусом тахини."
            },
            new Recipe
            {
                Id = 107,
                Title = "Запеченное яблоко с корицей",
                Description = "Теплый уютный десерт, богатый пектином для здорового пищеварения.",
                ImageUrl = "https://picsum.photos/id/429/600/400",
                Ingredients = "• Яблоко зеленое — 2 шт\n• Корица молотая — 3 г\n• Грецкий орех — 20 г\n• Мед — 10 г\n\nКБЖУ на 100г: 95 ккал | Б: 1.4г | Ж: 3.8г | У: 14.2г",
                CookingSteps = "1. Удалить сердцевину из яблок.\n2. Начинить орехами, корицей и каплей меда.\n3. Выпекать 20 минут при 180°C."
            },
            new Recipe
            {
                Id = 108,
                Title = "Салат со свеклой и пеканом",
                Description = "Сочный запеченный салат для поддержки кроветворения и сосудов.",
                ImageUrl = "https://picsum.photos/id/1084/600/400",
                Ingredients = "• Свекла запеченная — 150 г\n• Руккола — 30 г\n• Орех пекан — 15 г\n• Оливковое масло — 10 мл\n\nКБЖУ на 100г: 118 ккал | Б: 2.2г | Ж: 8.1г | У: 9.3г",
                CookingSteps = "1. Нарезать запеченную свеклу кубиками.\n2. Смешать с рукколой и орехами пекан.\n3. Сбрызнуть оливковым маслом."
            },
            new Recipe
            {
                Id = 109,
                Title = "Гречневые галеты с гуакамоле",
                Description = "Низкоуглеводная закуска с полезными мононенасыщенными жирами.",
                ImageUrl = "https://picsum.photos/id/30/600/400",
                Ingredients = "• Хлебцы гречневые — 3 шт\n• Авокадо — 1 шт\n• Сок лайма — 10 мл\n• Кинза — 5 г\n\nКБЖУ на 100г: 175 ккал | Б: 3.5г | Ж: 12.1г | У: 14.0г",
                CookingSteps = "1. Размять авокадо вилкой с соком лайма и кинзой.\n2. Нанести гуакамоле на гречневые хлебцы."
            },
            new Recipe
            {
                Id = 110,
                Title = "Лимонно-имбирный детокс-эликсир",
                Description = "Тонизирующий напиток для разогрева метаболизма и щелочного баланса.",
                ImageUrl = "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Сок лимона — 30 мл\n• Корень имбиря тертый — 10 г\n• Вода теплая — 250 мл\n• Куркума — 2 г\n\nКБЖУ на 100г: 18 ккал | Б: 0.3г | Ж: 0.1г | У: 3.8г",
                CookingSteps = "1. Настоять имбирь в теплой воде 5 минут.\n2. Процедить, добавить лимонный сок и куркуму.\n3. Пить с утра натощак."
            },
            new Recipe
            {
                Id = 111,
                Title = "Паста из цукини с черри и базиликом",
                Description = "Легкая низкокалорийная альтернатива классической итальянской пасте.",
                ImageUrl = "https://images.unsplash.com/photo-1511690656952-34342bb7c2f2?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Цукини свежие — 200 г\n• Томаты черри — 80 г\n• Базилик свежий — 15 г\n• Кедровые орехи — 10 г\n\nКБЖУ на 100г: 62 ккал | Б: 1.8г | Ж: 3.5г | У: 6.1г",
                CookingSteps = "1. Нарезать цукини спиральной соломкой.\n2. Слегка прогреть на сковороде 2 минуты с черри.\n3. Украсить свежим базиликом."
            }
        };

        private static readonly List<PriceGroup> PriceGroups = new()
        {
            new PriceGroup { Id = 1, Name = "Чистое питание", Description = "Базовые блюда, супы, горячее и соусы", PriceUah = 450 },
            new PriceGroup { Id = 2, Name = "Живое тесто & Сладкое", Description = "Безглютеновый хлеб, выпечка, десерты", PriceUah = 650 },
            new PriceGroup { Id = 3, Name = "Суперфуды & Смузи", Description = "Детокс-напитки, витаминные боулы и эликсиры", PriceUah = 350 }
        };

        // Коллекция из 18 ПЛАТНЫХ рецептов
        public static readonly List<Recipe> Recipes = new()
        {
            new Recipe {
                Id = 1, PriceGroupId = 1, Title = "Тако-блин из киноа с авокадо",
                Description = "Питательный блин без муки и глютена для био-хакинг завтрака.",
                ImageUrl = "https://picsum.photos/id/1080/600/400",
                VideoUrl = "/Video/sample.mp4",
                Ingredients = "• Киноа белая — 120 г\n• Вода — 90 мл\n• Семена льна — 10 г\n• Авокадо — 0.5 шт\n\nКБЖУ на 100г: 185 ккал | Б: 6.2г | Ж: 7.1г | У: 24.5г",
                CookingSteps = "1. Замочить киноа на 4 часа.\n2. Взбить в блендере с водой до гладкого теста.\n3. Обжарить на сковороде по 3 мин с каждой стороны."
            },
            new Recipe {
                Id = 2, PriceGroupId = 1, Title = "Крем-суп из брокколи и цукини",
                Description = "Легкий детокс-суп на кокосовом молоке с микрозеленью.",
                ImageUrl = "https://picsum.photos/id/225/600/400",
                Ingredients = "• Брокколи — 300 г\n• Цукини — 1 шт\n• Кокосовое молоко — 150 мл\n• Чеснок, шпинат — по вкусу\n\nКБЖУ на 100г: 92 ккал | Б: 3.1г | Ж: 5.4г | У: 8.2г",
                CookingSteps = "1. Отварить брокколи и цукини на пару 10 минут.\n2. Переложить в блендер, добавить кокосовое молоко и специи.\n3. Взбить до кремовой текстуры."
            },
            new Recipe {
                Id = 3, PriceGroupId = 1, Title = "Запеченный лосось в травах",
                Description = "Сочное филе лосося с заправкой из лимона и розмарина.",
                ImageUrl = "https://picsum.photos/id/1084/600/400",
                VideoUrl = "/Video/sample2.mp4",
                Ingredients = "• Филе лосося — 200 г\n• Лимонный сок — 15 мл\n• Розмарин, тимьян — по 2 веточки\n• Масло оливковое — 10 мл\n\nКБЖУ на 100г: 210 ккал | Б: 20.5г | Ж: 13.8г | У: 0.5г",
                CookingSteps = "1. Замариновать лосось в лимонном соке и травах на 15 мин.\n2. Запекать при 180°C в духовке 12-15 минут.\n3. Подавать с долькой лимона."
            },
            new Recipe {
                Id = 4, PriceGroupId = 1, Title = "Теплый боул с нутом и бататом",
                Description = "Сбалансированный обед с медленными углеводами и растительным белком.",
                ImageUrl = "https://picsum.photos/id/1062/600/400",
                Ingredients = "• Нут отварной — 100 г\n• Батат запеченный — 120 г\n• Шпинат свежий — 40 г\n• Тахини — 15 г\n\nКБЖУ на 100г: 145 ккал | Б: 5.8г | Ж: 4.2г | У: 22.1г",
                CookingSteps = "1. Нарезать батат кубиками и запечь 20 мин.\n2. Прогреть нут на сковороде со специями.\n3. Собрать боул: зелень, батат, нут и полить соусом тахини."
            },
            new Recipe {
                Id = 9, PriceGroupId = 1, Title = "Салат с индейкой и овощами",
                Description = "Легкий протеиновый ужин с запеченным цукини и перцем.",
                ImageUrl = "https://picsum.photos/id/493/600/400",
                Ingredients = "• Филе индейки — 150 г\n• Перец болгарский — 1 шт\n• Руккола — 50 г\n• Оливковое масло — 10 мл\n\nКБЖУ на 100г: 125 ккал | Б: 16.2г | Ж: 4.5г | У: 5.1г",
                CookingSteps = "1. Запечь филе индейки и овощи на гриле.\n2. Нарезать ломтиками и выложить на подушку из рукколы.\n3. Сбрызнуть оливковым маслом и бальзамиком."
            },
            new Recipe {
                Id = 11, PriceGroupId = 1, Title = "Паста из цукини с песто",
                Description = "Низкоуглеводная «паста» с густым зеленым соусом и орехами.",
                ImageUrl = "https://picsum.photos/id/326/600/400",
                Ingredients = "• Цукини свежие — 2 шт\n• Авокадо спелое — 1 шт\n• Базилик зеленый — 30 г\n• Кедровые орехи — 20 г\n\nКБЖУ на 100г: 115 ккал | Б: 3.4г | Ж: 9.1г | У: 6.2г",
                CookingSteps = "1. Нарезать цукини тонкой соломкой.\n2. Взбить в блендере авокадо, базилик, орехи и оливковое масло.\n3. Смешать цукини с соусом и подать слегка согретым."
            },
            new Recipe {
                Id = 5, PriceGroupId = 2, Title = "Миндальный чизкейк с кокосом",
                Description = "RAW-десерт на ореховой основе без сахара и выпечки.",
                ImageUrl = "https://picsum.photos/id/102/600/400",
                Ingredients = "• Миндаль сырой — 150 г\n• Финики — 100 г\n• Кешью — 200 г\n• Кокосовое молоко — 120 мл\n\nКБЖУ на 100г: 320 ккал | Б: 8.5г | Ж: 22.1г | У: 21.0г",
                CookingSteps = "1. Измельчить миндаль с финиками для основы.\n2. Взбить кешью с кокосовым молоком в крем.\n3. Убрать в морозильную камеру на 3-4 часа."
            },
            new Recipe {
                Id = 6, PriceGroupId = 2, Title = "Безглютеновый гречневый хлеб",
                Description = "Пышный домашний хлеб на зеленой гречке и псиллиуме.",
                ImageUrl = "https://picsum.photos/id/30/600/400",
                Ingredients = "• Зеленая гречка — 300 г\n• Псиллиум (шелуха) — 20 г\n• Вода — 200 мл\n• Семена тыквы — 30 г\n\nКБЖУ на 100г: 165 ккал | Б: 6.1г | Ж: 2.8г | У: 30.4г",
                CookingSteps = "1. Замочить зеленую гречку на 8 часов.\n2. Измельчить в блендере с псиллиумом и водой.\n3. Выпекать в форме 55 минут при 180°C."
            },
            new Recipe {
                Id = 7, PriceGroupId = 2, Title = "Ягодные оладьи на муке кассавы",
                Description = "Воздушные оладьи с черникой без лактозы и сахара.",
                ImageUrl = "https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Мука кассавы — 100 г\n• Кокосовые сливки — 100 мл\n• Яйца перепелиные — 4 шт\n• Черника свежая — 50 г\n\nКБЖУ на 100г: 210 ккал | Б: 4.8г | Ж: 9.2г | У: 26.0г",
                CookingSteps = "1. Смешать муку, сливки и яйца до жидкого теста.\n2. Всыпать чернику.\n3. Выпекать на антипригарной сковороде по 2 мин."
            },
            new Recipe {
                Id = 8, PriceGroupId = 2, Title = "Шоколадно-авокадовый мусс",
                Description = "Нежнейший десерт с кэробом и спелым авокадо.",
                ImageUrl = "https://picsum.photos/id/431/600/400",
                Ingredients = "• Авокадо спелое — 2 шт\n• Кэроб или какао — 30 г\n• Сироп топинамбура — 30 мл\n• Ваниль натуральная — 1 г\n\nКБЖУ на 100г: 195 ккал | Б: 3.2г | Ж: 14.5г | У: 15.8г",
                CookingSteps = "1. Очистить авокадо и выложить в чашу блендера.\n2. Добавить кэроб, сироп топинамбура и ваниль.\n3. Взбить до состояния шелковистого мусса и охладить."
            },
            new Recipe {
                Id = 10, PriceGroupId = 2, Title = "Кокосовый тарт с малиной",
                Description = "Хрустящая песочная основа из миндальной муки и малиновый соус.",
                ImageUrl = "https://picsum.photos/id/429/600/400",
                Ingredients = "• Миндальная мука — 120 г\n• Кокосовое масло — 40 мл\n• Малина свежая — 150 г\n• Агар-агар — 4 г\n\nКБЖУ на 100г: 240 ккал | Б: 5.1г | Ж: 17.2г | У: 16.5г",
                CookingSteps = "1. Замесить тесто из миндальной муки и кокосового масла, выпечь корзинку 12 мин.\n2. Приготовить прослойку из малины и агар-агара.\n3. Залить начинку в корзинку и охладить до застывания."
            },
            new Recipe {
                Id = 12, PriceGroupId = 2, Title = "Матча-конфеты с кешью",
                Description = "Энергетические суперфуд-трюфели с чаем матча и кокосом.",
                ImageUrl = "https://picsum.photos/id/1060/600/400",
                Ingredients = "• Кешью сырой — 150 г\n• Кокосовая стружка — 50 г\n• Японский чай матча — 10 г\n• Мед натуральный — 30 мл\n\nКБЖУ на 100г: 285 ккал | Б: 7.2г | Ж: 18.5г | У: 22.0г",
                CookingSteps = "1. Измельчить кешью с кокосовой стружкой и чаем матча в блендере.\n2. Добавить мед и замесить плотную массу.\n3. Скатать небольшие шарики и убрать в холодильник на 1 час."
            },
            new Recipe {
                Id = 13, PriceGroupId = 3, Title = "Зеленый детокс-смузи",
                Description = "Мощный заряд хлорофилла для клеточного обновления.",
                ImageUrl = "https://images.unsplash.com/photo-1610970881699-44a5587cabec?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Сельдерей стебли — 2 шт\n• Яблоко зеленое — 1 шт\n• Шпинат свежий — 50 г\n• Порошок спирулины — 5 г\n\nКБЖУ на 100г: 45 ккал | Б: 1.8г | Ж: 0.3г | У: 9.2г",
                CookingSteps = "1. Нарезать сельдерей и яблоко.\n2. Выложить ингредиенты в блендер.\n3. Взбивать 60 секунд до однородности."
            },
            new Recipe {
                Id = 14, PriceGroupId = 3, Title = "Асаи-боул с манго",
                Description = "Антиоксидантный завтрак на основе ягод асаи.",
                ImageUrl = "https://images.unsplash.com/photo-1590301157890-4810ed352733?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Пюре асаи — 100 г\n• Банан — 1 шт\n• Манго свежее — 50 г\n• Гранола — 30 г\n\nКБЖУ на 100г: 128 ккал | Б: 2.1г | Ж: 3.5г | У: 22.4г",
                CookingSteps = "1. Взбить асаи с бананом в крем.\n2. Выложить в пиалу.\n3. Украсить манго и гранолой."
            },
            new Recipe {
                Id = 15, PriceGroupId = 3, Title = "Золотое молоко с куркумой",
                Description = "Противовоспалительный тонизирующий напиток «Golden Milk».",
                ImageUrl = "https://images.unsplash.com/photo-1544787219-7f47ccb76574?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Кокосовое молоко — 250 мл\n• Куркума — 5 г\n• Имбирь тертый — 3 г\n• Перец черный — 1 щепотка\n\nКБЖУ на 100г: 88 ккал | Б: 1.1г | Ж: 8.2г | У: 2.8г",
                CookingSteps = "1. Прогреть молоко с куркумой и имбирем.\n2. Взбить капучинатором до пенки.\n3. Подать горячим."
            },
            new Recipe {
                Id = 16, PriceGroupId = 3, Title = "Чиа-пудинг с маракуйей",
                Description = "Омега-3 суперфуд с прослойкой из пюре маракуйи.",
                ImageUrl = "https://images.unsplash.com/photo-1511690656952-34342bb7c2f2?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Семена чиа — 30 г\n• Кокосовое молоко — 150 мл\n• Пюре маракуйи — 40 г\n\nКБЖУ на 100г: 155 ккал | Б: 4.2г | Ж: 9.8г | У: 13.1г",
                CookingSteps = "1. Залить чиа кокосовым молоком.\n2. Оставить в холодильнике на 4 часа.\n3. Украсить пюре маракуйи."
            },
            new Recipe {
                Id = 17, PriceGroupId = 3, Title = "Ягодный смузи с макой",
                Description = "Энергетический микс для адаптации организма к стрессу.",
                ImageUrl = "https://images.unsplash.com/photo-1553530666-ba11a7da3888?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Ягоды микс — 100 г\n• Миндальное молоко — 200 мл\n• Порошок маки — 5 г\n\nКБЖУ на 100г: 62 ккал | Б: 2.0г | Ж: 2.4г | У: 8.5г",
                CookingSteps = "1. Поместить ягоды и маку в блендер.\n2. Влить миндальное молоко.\n3. Взбить до эмульсии."
            },
            new Recipe {
                Id = 18, PriceGroupId = 3, Title = "Шот с витграссом и лаймом",
                Description = "Концентрированный иммунный бустер с соком ростков пшеницы.",
                ImageUrl = "https://images.unsplash.com/photo-1613478223719-2ab802602423?auto=format&fit=crop&w=600&q=80",
                Ingredients = "• Витграсс — 30 мл\n• Сок грейпфрута — 50 мл\n• Сок лайма — 15 мл\n\nКБЖУ на 100г: 35 ккал | Б: 1.1г | Ж: 0.1г | У: 7.8г",
                CookingSteps = "1. Отжать цитрусовый сок.\n2. Смешать с витграссом.\n3. Подать в шоте."
            }
        };

        // GET: /recipes/free
        [HttpGet("recipes/free")]
        public IActionResult FreeRecipes()
        {
            return View(FreeRecipesList);
        }

        // GET: /recipes/paid
        [HttpGet("recipes/paid")]
        public async Task<IActionResult> PaidRecipes()
        {
            int[]? overrideActiveGroupIds = null;

            Dictionary<int, DateTime> activeSubscriptions = new();

            if (overrideActiveGroupIds != null)
            {
                foreach (var id in overrideActiveGroupIds)
                {
                    activeSubscriptions[id] = DateTime.UtcNow.AddDays(30);
                }
            }
            else if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // АВТОМАТИЧЕСКИЙ ПОЛНЫЙ ДОСТУП ДЛЯ РОЛИ ADMIN
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        foreach (var group in PriceGroups)
                        {
                            activeSubscriptions[group.Id] = DateTime.UtcNow.AddYears(99);
                        }
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

            var viewModel = PriceGroups.Select(g => new PaidGroupViewModel
            {
                Group = g,
                HasAccess = activeSubscriptions.ContainsKey(g.Id),
                ExpirationDate = activeSubscriptions.ContainsKey(g.Id) ? activeSubscriptions[g.Id] : null,
                Recipes = Recipes.Where(r => r.PriceGroupId == g.Id).ToList()
            }).ToList();

            return View(viewModel);
        }
    }
}