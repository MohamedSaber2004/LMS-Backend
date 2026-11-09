using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Infrastructure.Services.LiveKit
{
    public class LiveKitTokenService : ILiveClassTokenService
    {
        private readonly LiveKitSettings _settings;
        private readonly ILogger<LiveKitTokenService> _logger;

        public LiveKitTokenService(IOptions<LiveKitSettings> options, ILogger<LiveKitTokenService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => _settings.HasCredentials;

        public string Url => _settings.Url ?? string.Empty;

        public int JoinLeadMinutes => _settings.JoinLeadMinutes > 0 ? _settings.JoinLeadMinutes : 15;

        public int TokenLifetimeMinutes => _settings.TokenLifetimeMinutes > 0 ? _settings.TokenLifetimeMinutes : 1440;

        public uint GenerateParticipantUid(Guid userId)
        {
            byte[] bytes = userId.ToByteArray();
            uint raw = BitConverter.ToUInt32(bytes, 0);
            if (raw == 0)
            {
                raw = 1;
            }
            return raw;
        }

        public string GenerateAccessToken(string roomName, uint participantUid, bool canPublish, int? expireSeconds = null)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Cannot generate live class token: LiveKit credentials are not configured.");
                return string.Empty;
            }

            int ttlSeconds = expireSeconds.HasValue && expireSeconds.Value > 0
                ? expireSeconds.Value
                : TokenLifetimeMinutes * 60;

            var now = DateTimeOffset.UtcNow;
            var identity = participantUid.ToString(CultureInfo.InvariantCulture);

            var videoGrant = new Dictionary<string, object>
            {
                ["room"] = roomName,
                ["roomJoin"] = true,
                ["canPublish"] = canPublish,
                ["canSubscribe"] = true,
                ["canPublishData"] = canPublish
            };

            var payload = new Dictionary<string, object>
            {
                ["iss"] = _settings.ApiKey,
                ["sub"] = identity,
                ["nbf"] = now.ToUnixTimeSeconds(),
                ["exp"] = now.AddSeconds(ttlSeconds).ToUnixTimeSeconds(),
                ["jti"] = Guid.NewGuid().ToString("N"),
                ["video"] = videoGrant
            };

            var signingKey = Encoding.UTF8.GetBytes(_settings.ApiSecret);
            var unsigned = Base64UrlEncoder.Encode("{\"alg\":\"HS256\",\"typ\":\"JWT\"}")
                + "."
                + Base64UrlEncoder.Encode(JsonSerializer.Serialize(payload));

            using var hmac = new HMACSHA256(signingKey);
            var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(unsigned));

            return unsigned + "." + Base64UrlEncoder.Encode(signature);
        }
    }
}
