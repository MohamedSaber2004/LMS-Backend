using LMS.Domain.Enums;

namespace LMS.Application.Features.Courses.DTOs
{
    public class QuizOptionAdminDto
    {
        public QuizOptionAdminDto() { }

        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int Order { get; set; }
    }

    public class QuizQuestionAdminDto
    {
        public QuizQuestionAdminDto() { }

        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public IReadOnlyCollection<QuizOptionAdminDto> Options { get; set; } = Array.Empty<QuizOptionAdminDto>();
    }

    public class LessonAdminDto
    {
        public LessonAdminDto() { }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string? StudyBookUrl { get; set; }
        public int Order { get; set; }
        public bool IsFreeDemo { get; set; }
        public bool IsActive { get; set; }
        public Guid? SessionId { get; set; }
        public DateTime? SessionStartsAtUtc { get; set; }
        public DateTime? SessionEndsAtUtc { get; set; }
        public LiveRoomStatus? LiveRoomStatus { get; set; }
        public IReadOnlyCollection<QuizQuestionAdminDto> QuizQuestions { get; set; } = Array.Empty<QuizQuestionAdminDto>();
    }

    public class CourseAdminDetailDto
    {
        public CourseAdminDetailDto() { }

        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public CourseLevel Level { get; set; }
        public CourseType Type { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public IReadOnlyCollection<InstructorRefDto> Instructors { get; set; } = Array.Empty<InstructorRefDto>();
        public IReadOnlyCollection<LessonAdminDto> Lessons { get; set; } = Array.Empty<LessonAdminDto>();
    }
}
