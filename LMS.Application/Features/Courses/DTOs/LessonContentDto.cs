namespace LMS.Application.Features.Courses.DTOs
{
    public class QuizOptionDto
    {
        public QuizOptionDto() { }

        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class QuizQuestionDto
    {
        public QuizQuestionDto() { }

        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
        public IReadOnlyCollection<QuizOptionDto> Options { get; set; } = Array.Empty<QuizOptionDto>();
    }

    public class LessonProgressDto
    {
        public LessonProgressDto() { }

        public int LastWatchedSeconds { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class QuizAttemptAnswerDto
    {
        public QuizAttemptAnswerDto() { }

        public Guid QuestionId { get; set; }
        public Guid SelectedOptionId { get; set; }
    }

    public class QuizResultDto
    {
        public QuizResultDto() { }

        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAtUtc { get; set; }
        public IReadOnlyCollection<QuizAttemptAnswerDto> Answers { get; set; } = Array.Empty<QuizAttemptAnswerDto>();
    }

    public class LessonContentDto
    {
        public LessonContentDto() { }

        public Guid CourseId { get; set; }
        public Guid LessonId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsFreeDemo { get; set; }
        public string VideoUrl { get; set; } = string.Empty;
        public string? StudyBookUrl { get; set; }
        public LessonProgressDto? Progress { get; set; }
        public IReadOnlyCollection<QuizQuestionDto> QuizQuestions { get; set; } = Array.Empty<QuizQuestionDto>();
        public QuizResultDto? LastAttempt { get; set; }
    }
}
