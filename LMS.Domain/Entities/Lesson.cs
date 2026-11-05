using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class Lesson : BaseEntity<Guid>
    {
        public Guid CourseId { get; private set; }
        public virtual Course Course { get; private set; } = null!;

        public string Title { get; private set; } = string.Empty;

        public string VideoUrl { get; private set; } = string.Empty;

        public string? StudyBookUrl { get; private set; }

        public int Order { get; private set; }

        public bool IsFreeDemo { get; private set; }

        public virtual ICollection<QuizQuestion> QuizQuestions { get; private set; } = new List<QuizQuestion>();

        public static Lesson Create(
            Guid courseId,
            string title,
            string videoUrl,
            string? studyBookUrl,
            int order,
            bool isFreeDemo)
        {
            return new Lesson
            {
                CourseId = courseId,
                Title = title,
                VideoUrl = videoUrl,
                StudyBookUrl = studyBookUrl,
                Order = order,
                IsFreeDemo = isFreeDemo
            };
        }

        public void Update(
            string title,
            string videoUrl,
            string? studyBookUrl,
            int order,
            bool isFreeDemo)
        {
            Title = title;
            VideoUrl = videoUrl;
            StudyBookUrl = studyBookUrl;
            Order = order;
            IsFreeDemo = isFreeDemo;
        }
    }
}
