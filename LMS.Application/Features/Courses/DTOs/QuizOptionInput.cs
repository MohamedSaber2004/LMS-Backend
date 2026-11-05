namespace LMS.Application.Features.Courses.DTOs
{
    public class QuizOptionInput
    {
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public int Order { get; set; }
    }
}
