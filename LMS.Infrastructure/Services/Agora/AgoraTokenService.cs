using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Infrastructure.Services.Agora.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LMS.Infrastructure.Services.Agora
{
    public class AgoraTokenService : IAgoraTokenService
    {
        private readonly AgoraSettings _settings;
        private readonly ILogger<AgoraTokenService> _logger;

        public AgoraTokenService(IOptions<AgoraSettings> options, ILogger<AgoraTokenService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public bool IsConfigured => _settings.HasCredentials;

        public string AppId => _settings.AppId ?? string.Empty;

        public int JoinLeadMinutes => _settings.JoinLeadMinutes > 0 ? _settings.JoinLeadMinutes : 15;

        public uint GenerateAgoraUid(Guid userId)
        {
            byte[] bytes = userId.ToByteArray();
            uint raw = BitConverter.ToUInt32(bytes, 0);
            if (raw == 0)
            {
                raw = 1;
            }
            return raw;
        }

        public string GenerateRtcToken(string channelName, uint agoraUid, bool isPublisher, int? expireSeconds = null)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("Cannot generate Agora token: Agora credentials are not configured.");
                return string.Empty;
            }

            uint expiration = expireSeconds.HasValue && expireSeconds.Value > 0
                ? (uint)expireSeconds.Value
                : (uint)(_settings.TokenLifetimeMinutes > 0 ? _settings.TokenLifetimeMinutes * 60 : 86400);

            var role = isPublisher ? RtcTokenBuilder2.Role.RolePublisher : RtcTokenBuilder2.Role.RoleSubscriber;

            return RtcTokenBuilder2.BuildTokenWithUid(
                _settings.AppId,
                _settings.AppCertificate,
                channelName,
                agoraUid,
                role,
                expiration,
                expiration);
        }
    }
}

