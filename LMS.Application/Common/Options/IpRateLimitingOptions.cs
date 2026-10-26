namespace LMS.Application.Common.Options
{
    public sealed class IpRateLimitingOptions
    {
        public const string SectionName = "IpRateLimiting";
        public bool EnableEndpointRateLimiting { get; set; }
        public bool StackBlockedRequests { get; set; }
        public string ClientIdHeader { get; set; } = string.Empty;
        public int HttpStatusCode { get; set; } = 429;
        public List<IpRateLimitRule> GeneralRules { get; set; } = new();
    }

    public sealed class IpRateLimitRule
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public int Limit { get; set; }
    }
}
