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

        public int Order { get; private set; }

        public virtual ICollection<Lesson> Lessons { get; private set; } = new List<Lesson>();

        public static Course Create(string title, string description, string? coverUrl, CourseLevel level, int order)
        {
            return new Course
            {
                Title = title,
                Description = description,
                CoverUrl = coverUrl,
                Level = level,
                Order = order
            };
        }

        public void Update(string title, string description, string? coverUrl, CourseLevel level, int order)
        {
            Title = title;
            Description = description;
            CoverUrl = coverUrl;
            Level = level;
            Order = order;
        }
    }
}
