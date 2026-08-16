using System;

namespace NutriWeb.Models
{
    // Модель подписки пользователя на конкретный тарифный план
    public class UserSubscription
    {
        public int Id { get; set; }

        // Идентификатор пользователя из системы Identity
        public string UserId { get; set; } = string.Empty;

        // Идентификатор тарифной группы (1 — "Чистое питание", 2 — "Живое тесто & Сладкое", 3 — "Суперфуды & Смузи")
        public int PriceGroupId { get; set; }

        // Оплаченный период подписки в месяцах (1, 6 или 12)
        public int MonthsPeriod { get; set; } = 1;

        // Даты действия подписки
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpirationDate { get; set; }

        // Флаг активности подписки
        public bool IsActive => DateTime.UtcNow <= ExpirationDate;

        /// <summary>
        /// Метод расчета даты окончания подписки с учетом выбранного периода.
        /// Если текущая подписка еще активна, новый период прибавляется к ExpirationDate.
        /// </summary>
        public static DateTime CalculateExpirationDate(DateTime? currentExpirationDate, int monthsPeriod)
        {
            DateTime baseDate = (currentExpirationDate.HasValue && currentExpirationDate.Value > DateTime.UtcNow)
                ? currentExpirationDate.Value
                : DateTime.UtcNow;

            return baseDate.AddMonths(monthsPeriod > 0 ? monthsPeriod : 1);
        }
    }
}