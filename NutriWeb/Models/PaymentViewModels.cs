using System.ComponentModel.DataAnnotations;

namespace NutriWeb.Models
{
    // Модель для отображения и подтверждения оплаты тарифного плана
    public class CheckoutViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string GroupDescription { get; set; } = string.Empty;
        public decimal PriceUah { get; set; }

        // Период подписки в месяцах (1, 6 или 12 месяцев)
        public int MonthsPeriod { get; set; } = 1;

        // Метод оплаты: "Card", "GooglePay", "ApplePay"
        public string PaymentMethod { get; set; } = "Card";

        [Display(Name = "Имя на карте")]
        public string CardHolder { get; set; } = string.Empty;

        [Display(Name = "Номер карты")]
        public string CardNumber { get; set; } = string.Empty;

        [Display(Name = "Срок действия")]
        public string ExpiryDate { get; set; } = string.Empty;

        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV должен состоять из 3 цифр")]
        [Display(Name = "CVV код")]
        public string Cvv { get; set; } = string.Empty;
    }
}