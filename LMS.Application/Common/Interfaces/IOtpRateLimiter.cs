namespace LMS.Application.Common.Interfaces
{
    public interface IOtpRateLimiter
    {
        (bool IsAllowed, int RemainingSeconds) CheckAndRecord(string email, TimeSpan cooldown);
    }
}
