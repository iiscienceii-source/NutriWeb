namespace NutriWeb.Models
{
    // Модель рецепта
    public class Recipe
    {
        public int Id { get; set; }                      // Уникальный ID рецепта[cite: 1]
        public string Title { get; set; } = "";         // Название (видят все посетители)[cite: 1]
        public string Description { get; set; } = "";   // Краткое описание (видят все посетители)[cite: 1]
        public string Ingredients { get; set; } = "";   // Ингредиенты и КБЖУ (видно только по подписке)[cite: 1]
        public string CookingSteps { get; set; } = "";  // Технология приготовления (видно только по подписке)[cite: 1]
        public string ImageUrl { get; set; } = "";       // Ссылка на фото блюда[cite: 1]
        public bool IsPaid { get; set; } = true;        // Флаг: платный ли рецепт[cite: 1]
        public int PriceGroupId { get; set; }           // Привязка к тарифной группе[cite: 1]

        // Локальный путь к видеофайлу (хранится в wwwroot/Video/)
        public string? VideoUrl { get; set; }
    }

    // ViewModel для вывода подгрупп на странице платных рецептов
    public class PaidGroupViewModel
    {
        public PriceGroup Group { get; set; } = new();  // Информация о тарифе[cite: 1]
        public bool HasAccess { get; set; }             // Флаг доступа текущего аккаунта[cite: 1]
        public List<Recipe> Recipes { get; set; } = new(); // Список рецептов подгруппы[cite: 1]
    }
}