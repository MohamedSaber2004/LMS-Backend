using LMS.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace LMS.Infrastructure.Services.Authentication
{
    public class OtpRateLimiter : IOtpRateLimiter
    {
        private readonly IMemoryCache _cache;
        private static readonly object LockObj = new();

        public OtpRateLimiter(IMemoryCache cache)
        {
            _cache = cache;
        }

        public (bool IsAllowed, int RemainingSeconds) CheckAndRecord(string email, TimeSpan cooldown)
        {
            if (string.IsNullOrWhiteSpace(email))
                return (true, 0);

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var cacheKey = $"otp_resend_ratelimit_{normalizedEmail}";

            lock (LockObj)
            {
                if (_cache.TryGetValue(cacheKey, out DateTime sentAt))
                {
                    var elapsed = DateTime.UtcNow - sentAt;
                    if (elapsed < cooldown)
                    {
                        var remaining = (int)Math.Ceiling((cooldown - elapsed).TotalSeconds);
                        return (false, remaining > 0 ? remaining : 1);
                    }
                }

                _cache.Set(cacheKey, DateTime.UtcNow, cooldown);
                return (true, 0);
            }
        }
    }
}
