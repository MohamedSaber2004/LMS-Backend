namespace LMS.Application.Common.Options
{
    public class EmailSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int VerificationCodeExpiryMinutes { get; set; } = 10;
    }
}
