namespace LMS.Application.Common.Options
{
    public sealed class CorsOptions
    {
        public bool Enabled { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public List<string> AllowedOrigins { get; set; } = null!;
        public List<string> AllowedMethods { get; set; } = null!;
        public List<string> AllowedHeaders { get; set; } = null!;
        public bool AllowCredentials { get; set; }
    }
}
