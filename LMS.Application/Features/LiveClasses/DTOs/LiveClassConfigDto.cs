namespace LMS.Application.Features.LiveClasses.DTOs
{
    public class LiveClassConfigDto
    {
        public bool Enabled { get; set; }
        public string Url { get; set; } = string.Empty;
        public int JoinLeadMinutes { get; set; }
    }
}
