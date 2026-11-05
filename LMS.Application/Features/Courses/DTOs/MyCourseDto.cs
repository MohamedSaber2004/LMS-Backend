using LMS.Domain.Enums;

namespace LMS.Application.Features.Courses.DTOs
{
    public class MyCourseDto
    {
        public MyCourseDto() { }

        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public CourseLevel Level { get; set; }
        public int LessonCount { get; set; }
        public int CompletedLessonCount { get; set; }
        public Guid? LastLessonId { get; set; }
        public string? LastLessonTitle { get; set; }
        public int? LastWatchedSeconds { get; set; }
        public bool? LastLessonIsCompleted { get; set; }
        public DateTime EnrolledAtUtc { get; set; }
    }
}
