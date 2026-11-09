namespace LMS.Application.Common.Options
{
    public class LiveKitSettings
    {
        public const string SectionName = "LiveKit";

        public string Url { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string ApiSecret { get; set; } = null!;
        public int TokenLifetimeMinutes { get; set; } = 1440;
        public int JoinLeadMinutes { get; set; } = 15;

        public bool HasCredentials =>
            !string.IsNullOrWhiteSpace(Url) &&
            !string.IsNullOrWhiteSpace(ApiKey) &&
            !string.IsNullOrWhiteSpace(ApiSecret);
    }
}
