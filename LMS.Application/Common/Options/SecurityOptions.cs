namespace LMS.Application.Common.Options
{
    public sealed class SecurityOptions
    {
        public const string SectionName = "Security";

        public SecurityHeadersOptions Headers { get; set; } = null!;
        public CorsOptions Cors { get; set; } = null!;
        public AntiforgeryOptions Antiforgery { get; set; } = null!;
        public RequestLimitsOptions RequestLimits { get; set; } = null!;
        public HstsOptions Hsts { get; set; } = null!;
    }
}
