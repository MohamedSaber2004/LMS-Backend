namespace LMS.Application.Common.Options
{
    public class GoogleAuthSettings
    {
        public string WebClientId { get; set; } = null!;
        public string WebClientSecret { get; set; } = null!;

        public bool HasClientId => !string.IsNullOrWhiteSpace(WebClientId);
    }
}
