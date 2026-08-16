namespace NutriWeb.Models
{
    // Модель тарифной группы (для гибкой настройки подписок)
    public class PriceGroup
    {
        public int Id { get; set; }                     // Уникальный ID тарифной группы
        public string Name { get; set; } = "";        // Название тарифа (например: "Чистое питание")
        public string Description { get; set; } = ""; // Подробное описание тарифа
        public decimal PriceUah { get; set; }          // Стоимость подписки в гривнах
        public int DurationDays { get; set; } = 30;    // Срок действия подписки в днях
        public bool IsActive { get; set; } = true;    // Флаг активности тарифа
    }
}