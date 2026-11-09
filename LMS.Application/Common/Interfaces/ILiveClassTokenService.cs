namespace LMS.Application.Common.Interfaces
{
    public interface ILiveClassTokenService
    {
        bool IsConfigured { get; }
        string Url { get; }
        int JoinLeadMinutes { get; }
        int TokenLifetimeMinutes { get; }
        uint GenerateParticipantUid(Guid userId);
        string GenerateAccessToken(string roomName, uint participantUid, bool canPublish, int? expireSeconds = null);
    }
}
