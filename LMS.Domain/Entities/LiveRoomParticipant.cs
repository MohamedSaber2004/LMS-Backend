using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class LiveRoomParticipant : BaseEntity<Guid>
    {
        public Guid RoomId { get; private set; }
        public virtual LiveRoom Room { get; private set; } = null!;

        public Guid UserId { get; private set; }
        public virtual ApplicationUser User { get; private set; } = null!;

        public ParticipantRole Role { get; private set; }

        public uint AgoraUid { get; private set; }

        public DateTime JoinedAtUtc { get; private set; }

        public DateTime? LeftAtUtc { get; private set; }

        private LiveRoomParticipant() { }

        public static LiveRoomParticipant Create(
            Guid roomId,
            Guid userId,
            ParticipantRole role,
            uint agoraUid,
            DateTime joinedAtUtc)
        {
            return new LiveRoomParticipant
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                UserId = userId,
                Role = role,
                AgoraUid = agoraUid,
                JoinedAtUtc = joinedAtUtc
            };
        }

        public void Leave(DateTime leftAtUtc)
        {
            LeftAtUtc = leftAtUtc;
        }
    }
}

