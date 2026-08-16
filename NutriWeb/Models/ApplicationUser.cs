using Microsoft.AspNetCore.Identity;

namespace NutriWeb.Models
{
    // Расширенная модель пользователя на базе IdentityUser
    public class ApplicationUser : IdentityUser
    {
        // Полное имя пользователя (Имя и Фамилия)
        public string? FullName { get; set; }
    }
}