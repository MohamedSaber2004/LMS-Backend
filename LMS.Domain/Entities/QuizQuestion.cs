using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class QuizQuestion : BaseEntity<Guid>
    {
        public Guid? LessonId { get; private set; }
        public virtual Lesson? Lesson { get; private set; }

        public Guid? CourseId { get; private set; }
        public virtual Course? Course { get; private set; }

        public string Text { get; private set; } = string.Empty;

        public int Order { get; private set; }

        public virtual ICollection<QuizOption> Options { get; private set; } = new List<QuizOption>();

        public static QuizQuestion Create(Guid lessonId, string text, int order)
        {
            return new QuizQuestion
            {
                LessonId = lessonId,
                Text = text,
                Order = order
            };
        }

        public static QuizQuestion CreateForCourse(Guid courseId, string text, int order)
        {
            return new QuizQuestion
            {
                CourseId = courseId,
                Text = text,
                Order = order
            };
        }

        public void Update(string text, int order)
        {
            Text = text;
            Order = order;
        }

        public void ReplaceOptions(IEnumerable<QuizOption> options)
        {
            Options.Clear();

            foreach (var option in options)
            {
                Options.Add(option);
            }
        }
    }
}
