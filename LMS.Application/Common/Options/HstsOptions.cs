namespace LMS.Application.Common.Options
{
    public sealed class HstsOptions
    {
        public bool Enabled { get; set; }
        public bool Preload { get; set; }
        public bool IncludeSubDomains { get; set; }
        public int MaxAgeDays { get; set; }
    }
}
