using LMS.Domain.Enums;

namespace LMS.Application.Features.Courses.DTOs
{
    public class LessonListItemDto
    {
        public LessonListItemDto() { }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsFreeDemo { get; set; }
        public bool HasStudyBook { get; set; }
        public bool HasVideo { get; set; }
        public Guid? SessionId { get; set; }
        public DateTime? SessionStartsAtUtc { get; set; }
        public DateTime? SessionEndsAtUtc { get; set; }
        public LiveRoomStatus? LiveRoomStatus { get; set; }
        public bool CanJoinLiveRoom { get; set; }
    }

    public class CourseDetailDto
    {
        public CourseDetailDto() { }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public CourseLevel Level { get; set; }
        public CourseType Type { get; set; }
        public int Order { get; set; }
        public decimal Price { get; set; }
        public bool IsEnrolled { get; set; }
        public IReadOnlyCollection<InstructorRefDto> Instructors { get; set; } = Array.Empty<InstructorRefDto>();
        public IReadOnlyCollection<LessonListItemDto> Lessons { get; set; } = Array.Empty<LessonListItemDto>();
    }
}
