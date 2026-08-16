using System.ComponentModel.DataAnnotations;

namespace NutriWeb.Models
{
    // Модель для формы входа в аккаунт
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage = "Некорректный адрес Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    // Модель для формы регистрации нового пользователя
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите ваше имя")]
        [Display(Name = "Имя и Фамилия")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите Email")]
        [EmailAddress(ErrorMessage = "Некорректный адрес Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен быть не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}