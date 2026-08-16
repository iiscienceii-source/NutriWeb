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

                // Таймаут 10 секунд
                client.Timeout = 10000;

                // Для 587 порта в MailKit используем StartTls, для 465 - SslOnConnect
                var options = port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : SecureSocketOptions.StartTls;

                _logger.LogInformation("EmailSender: Подключение к {SmtpServer}:{Port}...", smtpServer, port);
                await client.ConnectAsync(smtpServer, port, options);

                _logger.LogInformation("EmailSender: Авторизация для {SenderEmail}...", senderEmail);
                await client.AuthenticateAsync(senderEmail, password);

                _logger.LogInformation("EmailSender: Отправка письма на {Email}...", email);
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
                _logger.LogInformation("EmailSender: Письмо успешно отправлено!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailSender: Сбой при отправке письма через MailKit!");
            }
        }
    }
}