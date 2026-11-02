namespace LMS.Application.Common.Interfaces
{
    public interface IEmailService
    {
        bool IsConfigured { get; }
        Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
        Task SendOtpEmailAsync(string to, string userName, string title, string message, string otpCode, int expiryMinutes, CancellationToken cancellationToken = default);
    }
}
