using LMS.Domain.Enums;

namespace LMS.Application.Features.Courses.DTOs
{
    public class CourseListItemDto
    {
        public CourseListItemDto() { }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public CourseLevel Level { get; set; }
        public int Order { get; set; }
        public int LessonCount { get; set; }
        public bool IsActive { get; set; }
    }
}
