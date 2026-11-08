namespace LMS.Application.Common.Options
{
    public class AgoraSettings
    {
        public const string SectionName = "Agora";

        public string AppId { get; set; } = null!;
        public string AppCertificate { get; set; } = null!;
        public int TokenLifetimeMinutes { get; set; } = 1440;
        public int JoinLeadMinutes { get; set; } = 15;

        public bool HasCredentials => !string.IsNullOrWhiteSpace(AppId) && !string.IsNullOrWhiteSpace(AppCertificate);
    }
}
