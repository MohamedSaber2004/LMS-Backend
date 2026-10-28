using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Application.Localization;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LMS.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILocalizationProvider _localizationProvider;
        private static string? _cachedOtpTemplate;

        public EmailService(
            IOptions<EmailSettings> emailOptions,
            ILocalizationProvider localizationProvider)
        {
            _emailSettings = emailOptions?.Value ?? new EmailSettings();
            _localizationProvider = localizationProvider;
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();
            var fromName = !string.IsNullOrWhiteSpace(_emailSettings.Name) ? _emailSettings.Name : "LMS Team";
            var fromAddress = !string.IsNullOrWhiteSpace(_emailSettings.Email) ? _emailSettings.Email : _emailSettings.Username;

            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            var secureSocketOptions = _emailSettings.Port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_emailSettings.Username) && !string.IsNullOrWhiteSpace(_emailSettings.Password))
            {
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        public async Task SendOtpEmailAsync(
            string to,
            string userName,
            string title,
            string message,
            string otpCode,
            int expiryMinutes,
            CancellationToken cancellationToken = default)
        {
            var fallbackUserName = _localizationProvider.GetLocalizedString(LocalizationKeys.Authentication.DefaultUserName);
            var effectiveUserName = !string.IsNullOrWhiteSpace(userName) ? userName : fallbackUserName;

            var greeting = _localizationProvider.GetLocalizedString(LocalizationKeys.Authentication.OtpEmailGreeting, null, effectiveUserName);
            var expiryNotice = _localizationProvider.GetLocalizedString(LocalizationKeys.Authentication.OtpEmailExpiryNotice, null, expiryMinutes);
            var footer = _localizationProvider.GetLocalizedString(LocalizationKeys.Authentication.OtpEmailFooter);

            var template = GetOtpTemplate();
            var htmlBody = template
                .Replace("{{Greeting}}", greeting)
                .Replace("{{UserName}}", effectiveUserName)
                .Replace("{{Title}}", title)
                .Replace("{{Message}}", message)
                .Replace("{{OtpCode}}", otpCode)
                .Replace("{{ExpiryMinutes}}", expiryMinutes.ToString())
                .Replace("{{ExpiryNotice}}", expiryNotice)
                .Replace("{{Footer}}", footer);

            await SendEmailAsync(to, title, htmlBody, cancellationToken);
        }

        private string GetOtpTemplate()
        {
            if (_cachedOtpTemplate != null)
                return _cachedOtpTemplate;

            var candidatePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Templates", "OtpEmailTemplate.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "Templates", "OtpEmailTemplate.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "LMS.Infrastructure", "Templates", "OtpEmailTemplate.txt"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "LMS.Infrastructure", "Templates", "OtpEmailTemplate.txt"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "LMS.Infrastructure", "Templates", "OtpEmailTemplate.txt")
            };

            foreach (var path in candidatePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        _cachedOtpTemplate = File.ReadAllText(path);
                        return _cachedOtpTemplate;
                    }
                }
                catch (Exception)
                {
                }
            }

            return _cachedOtpTemplate!;
        }
    }
}
