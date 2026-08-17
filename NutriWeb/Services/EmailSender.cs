using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace NutriWeb.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;
        private static readonly HttpClient HttpClient = new();

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _configuration["EmailSettings:ResendApiKey"];

            // На бесплатном тарифе Resend отправка идет от тестового адреса
            var senderEmail = "onboarding@resend.dev";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("EmailSender: API Ключ ResendApiKey не найден в Environment Variables.");
                return;
            }

            try
            {
                _logger.LogInformation("EmailSender: Отправка письма через Resend API на {Email}...", email);

                var payload = new
                {
                    from = $"NutriWeb <{senderEmail}>",
                    to = new[] { email },
                    subject = subject,
                    html = htmlMessage
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await HttpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("EmailSender: Письмо успешно доставлено через Resend API!");
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("EmailSender: Ошибка Resend API ({StatusCode}): {Error}", response.StatusCode, errorBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailSender: Критическое исключение при отправке через Resend API!");
            }
        }
    }
}