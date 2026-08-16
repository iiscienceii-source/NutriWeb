using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace NutriWeb.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";

            // Порт 587 используется по умолчанию для STARTTLS в System.Net.Mail.SmtpClient
            var portStr = _configuration["EmailSettings:Port"] ?? "587";
            int port = int.TryParse(portStr, out var p) ? p : 587;

            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var password = _configuration["EmailSettings:Password"]?.Replace(" ", "");

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("EmailSender: Учетные данные почты не заданы в Environment Variables.");
                return;
            }

            // Запускаем отправку в фоновой задаче, чтобы фронтенд сразу перенаправлял пользователя
            _ = Task.Run(async () =>
            {
                try
                {
                    using var client = new SmtpClient(smtpServer, port)
                    {
                        Credentials = new NetworkCredential(senderEmail, password),
                        EnableSsl = true,
                        Timeout = 5000 // Жёсткий таймаут 5 секунд
                    };

                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "NutriWeb"),
                        Subject = subject,
                        Body = htmlMessage,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("EmailSender: Письмо восстановления пароля отправлено на {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "EmailSender: Ошибка при отправке письма через SMTP на {Email}", email);
                }
            });

            await Task.CompletedTask;
        }
    }
}