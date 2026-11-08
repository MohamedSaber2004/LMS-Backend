namespace LMS.Application.Common.Interfaces
{
    public interface IAgoraTokenService
    {
        bool IsConfigured { get; }
        string AppId { get; }
        int JoinLeadMinutes { get; }
        uint GenerateAgoraUid(Guid userId);
        string GenerateRtcToken(string channelName, uint agoraUid, bool isPublisher, int? expireSeconds = null);
    }
}

