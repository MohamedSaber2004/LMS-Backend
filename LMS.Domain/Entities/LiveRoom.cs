using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class LiveRoom : BaseEntity<Guid>
    {
        public Guid SessionId { get; private set; }
        public virtual Session Session { get; private set; } = null!;

        public string ChannelName { get; private set; } = null!;

        public LiveRoomStatus Status { get; private set; }

        public DateTime? StartedAtUtc { get; private set; }

        public DateTime? EndedAtUtc { get; private set; }

        public Guid HostUserId { get; private set; }
        public virtual ApplicationUser HostUser { get; private set; } = null!;

        public virtual ICollection<LiveRoomParticipant> Participants { get; private set; } = new List<LiveRoomParticipant>();

        private LiveRoom() { }

        public static LiveRoom Create(Guid sessionId, Guid hostUserId, string? channelName = null)
        {
            return new LiveRoom
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                HostUserId = hostUserId,
                ChannelName = string.IsNullOrWhiteSpace(channelName) ? $"live-{sessionId:N}" : channelName,
                Status = LiveRoomStatus.NotStarted
            };
        }

        public void Start(DateTime startedAtUtc)
        {
            if (Status == LiveRoomStatus.Ended)
            {
                throw new InvalidOperationException("Cannot start an ended live room.");
            }

            Status = LiveRoomStatus.Live;
            StartedAtUtc = startedAtUtc;
        }

        public void End(DateTime endedAtUtc)
        {
            Status = LiveRoomStatus.Ended;
            EndedAtUtc = endedAtUtc;
        }

        public void MarkAbandoned(DateTime abandonedAtUtc)
        {
            Status = LiveRoomStatus.Abandoned;
            EndedAtUtc = abandonedAtUtc;
        }
    }
}

