namespace LMS.Application.Features.LiveClasses.DTOs
{
    public class LiveClassTokenDto
    {
        public Guid RoomId { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public uint Uid { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsHost { get; set; }
    }
}

