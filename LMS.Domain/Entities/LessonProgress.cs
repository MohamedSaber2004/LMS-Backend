using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class LessonProgress : BaseEntity<Guid>
    {
        public Guid LessonId { get; private set; }
        public virtual Lesson Lesson { get; private set; } = null!;

        public Guid LearnerId { get; private set; }
        public virtual Learner Learner { get; private set; } = null!;

        public int LastWatchedSeconds { get; private set; }

        public bool IsCompleted { get; private set; }

        public DateTime? CompletedAt { get; private set; }

        public static LessonProgress Create(Guid lessonId, Guid learnerId, int lastWatchedSeconds, bool isCompleted)
        {
            var progress = new LessonProgress
            {
                LessonId = lessonId,
                LearnerId = learnerId,
                LastWatchedSeconds = lastWatchedSeconds
            };

            progress.Record(lastWatchedSeconds, isCompleted);
            return progress;
        }

        public void Record(int lastWatchedSeconds, bool isCompleted)
        {
            LastWatchedSeconds = lastWatchedSeconds;
            IsCompleted = isCompleted;

            if (isCompleted)
            {
                CompletedAt ??= AppNow;
            }
        }
    }
}
