using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using NutriWeb.Models;

namespace NutriWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager, 
            IEmailSender emailSender,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Укажите ваш Email")]
            [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var cleanEmail = Input.Email.Trim();
                _logger.LogInformation("Попытка сброса пароля для Email: {Email}", cleanEmail);

                var user = await _userManager.FindByEmailAsync(cleanEmail);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с Email {Email} НЕ НАЙДЕН в базе данных!", cleanEmail);
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                _logger.LogInformation("Пользователь найден (Id: {Id}). Генерируем токен...", user.Id);

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code, email = cleanEmail },
                    protocol: Request.Scheme);

                _logger.LogInformation("Передаем письмо в EmailSender...");

                await _emailSender.SendEmailAsync(
                    cleanEmail,
                    "Восстановление пароля NutriWeb",
                    $"Для сброса пароля <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>перейдите по этой ссылке</a>.");

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}