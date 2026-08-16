using Microsoft.AspNetCore.Identity.UI.Services;

namespace NutriWeb.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Вывод информации об отправленном письме в окно отладки Visual Studio (Output)
            System.Diagnostics.Debug.WriteLine("==================================================");
            System.Diagnostics.Debug.WriteLine($"ОТПРАВКА E-MAIL НА: {email}");
            System.Diagnostics.Debug.WriteLine($"ТЕМА: {subject}");
            System.Diagnostics.Debug.WriteLine($"СОДЕРЖИМОЕ: {htmlMessage}");
            System.Diagnostics.Debug.WriteLine("==================================================");

            return Task.CompletedTask;
        }
    }
}