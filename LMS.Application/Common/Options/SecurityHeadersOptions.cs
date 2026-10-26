namespace LMS.Application.Common.Options
{
    public sealed class SecurityHeadersOptions
    {
        public bool Enabled { get; set; }
        public string XFrameOptions { get; set; } = string.Empty;
        public string XContentTypeOptions { get; set; } = string.Empty;
        public string XssProtection { get; set; } = string.Empty;
        public string ContentSecurityPolicy { get; set; } = string.Empty;
        public string ReferrerPolicy { get; set; } = string.Empty;
        public string PermissionsPolicy { get; set; } = string.Empty;
        public string StrictTransportSecurity { get; set; } = string.Empty;
    }
}
