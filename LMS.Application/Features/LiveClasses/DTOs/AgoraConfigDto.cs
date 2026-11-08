namespace LMS.Application.Features.LiveClasses.DTOs
{
    public class AgoraConfigDto
    {
        public bool Enabled { get; set; }
        public string AppId { get; set; } = string.Empty;
        public int JoinLeadMinutes { get; set; }
    }
}

