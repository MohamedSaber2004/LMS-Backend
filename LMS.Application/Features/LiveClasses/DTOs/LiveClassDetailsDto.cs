using System;
using System.Collections.Generic;

namespace LMS.Application.Features.LiveClasses.DTOs
{
    public class LiveClassDetailsDto
    {
        public Guid SessionId { get; set; }
        public Guid? RoomId { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool CanJoinNow { get; set; }
        public DateTime JoinOpensAtUtc { get; set; }
        public DateTime StartsAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? EndedAtUtc { get; set; }
        public bool IsHost { get; set; }
        public int ActiveParticipantsCount { get; set; }
        public List<ParticipantDto> Participants { get; set; } = new();
    }

    public class ParticipantDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public uint AgoraUid { get; set; }
        public DateTime JoinedAtUtc { get; set; }
        public bool IsOnline { get; set; }
    }
}

