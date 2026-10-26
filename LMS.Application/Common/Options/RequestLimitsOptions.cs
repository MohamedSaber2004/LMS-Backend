namespace LMS.Application.Common.Options
{
    public sealed class RequestLimitsOptions
    {
        public bool Enabled { get; set; }
        public long MaxRequestSizeBytes { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
