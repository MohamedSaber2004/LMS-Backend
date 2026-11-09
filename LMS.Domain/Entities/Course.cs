using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class Course : BaseEntity<Guid>
    {
        public string Title { get; private set; } = string.Empty;

        public string Description { get; private set; } = string.Empty;

        public string? CoverUrl { get; private set; }

        public CourseLevel Level { get; private set; }

        public CourseType Type { get; private set; }

        public int Order { get; private set; }

        public virtual ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();

        public virtual ICollection<CourseInstructor> CourseInstructors { get; private set; } = new List<CourseInstructor>();

        public virtual ICollection<CourseEnrollment> Enrollments { get; private set; } = new List<CourseEnrollment>();

        public virtual ICollection<QuizQuestion> FinalQuizQuestions { get; private set; } = new List<QuizQuestion>();

        public virtual ICollection<QuizAttempt> QuizAttempts { get; private set; } = new List<QuizAttempt>();

        public static Course Create(
            string title,
            string description,
            string? coverUrl,
            CourseLevel level,
            int order,
            CourseType type = CourseType.Live)
        {
            return new Course
            {
                Title = title,
                Description = description,
                CoverUrl = coverUrl,
                Level = level,
                Order = order,
                Type = type
            };
        }

        public void Update(
            string title,
            string description,
            string? coverUrl,
            CourseLevel level,
            int order,
            CourseType type = CourseType.Live)
        {
            Title = title;
            Description = description;
            CoverUrl = coverUrl;
            Level = level;
            Order = order;
            Type = type;
        }
    }
}
