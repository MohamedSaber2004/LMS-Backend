namespace LMS.Application.Common.Options
{
    public sealed class AntiforgeryOptions
    {
        public bool Enabled { get; set; }
        public bool ValidateOnAllNonIdempotentActions { get; set; }
        public string HeaderName { get; set; } = string.Empty;
        public string CookieName { get; set; } = string.Empty;
        public bool CookieHttpOnly { get; set; }
        public string CookieSameSite { get; set; } = string.Empty;
        public bool RequireSsl { get; set; }
    }
}
