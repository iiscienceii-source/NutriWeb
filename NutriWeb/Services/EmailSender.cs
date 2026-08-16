using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

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
            var portStr = _configuration["EmailSettings:Port"] ?? "587";
            int port = int.TryParse(portStr, out var p) ? p : 587;

            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var password = _configuration["EmailSettings:Password"]?.Replace(" ", "");

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("EmailSender: Учетные данные почты не заданы в Environment Variables.");
                return;
            }

            // Отправляем асинхронно в фоновой задаче
            _ = Task.Run(async () =>
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("NutriWeb", senderEmail));
                    message.To.Add(new MailboxAddress("", email));
                    message.Subject = subject;

                    var bodyBuilder = new BodyBuilder
                    {
                        HtmlBody = htmlMessage
                    };
                    message.Body = bodyBuilder.ToMessageBody();

                    using var client = new SmtpClient();

                    // Выбираем режим безопасности в зависимости от порта
                    var secureSocketOptions = port == 465
                        ? SecureSocketOptions.SslOnConnect
                        : SecureSocketOptions.StartTls;

                    await client.ConnectAsync(smtpServer, port, secureSocketOptions);
                    await client.AuthenticateAsync(senderEmail, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation("EmailSender: Письмо восстановления пароля успешно отправлено на {Email}", email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "EmailSender: Ошибка при отправке письма через MailKit на {Email}", email);
                }
            });

            await Task.CompletedTask;
        }
    }
}