using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class SessionReview : BaseEntity<Guid>
    {
        public Guid SessionId { get; private set; }
        public virtual Session Session { get; private set; } = null!;

        public Guid LearnerId { get; private set; }
        public virtual Learner Learner { get; private set; } = null!;

        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public int Rating { get; private set; } // 1 to 5
        public string? Comment { get; private set; }

        public static SessionReview Create(
            Guid sessionId,
            Guid learnerId,
            Guid instructorId,
            int rating,
            string? comment = null)
        {
            return new SessionReview
            {
                SessionId = sessionId,
                LearnerId = learnerId,
                InstructorId = instructorId,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
            };
        }
    }
}

