using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class QuizOption : BaseEntity<Guid>
    {
        public Guid QuizQuestionId { get; private set; }
        public virtual QuizQuestion QuizQuestion { get; private set; } = null!;

        public string Text { get; private set; } = string.Empty;

        public bool IsCorrect { get; private set; }

        public int Order { get; private set; }

        public static QuizOption Create(Guid quizQuestionId, string text, bool isCorrect, int order)
        {
            return new QuizOption
            {
                QuizQuestionId = quizQuestionId,
                Text = text,
                IsCorrect = isCorrect,
                Order = order
            };
        }

        public void Update(string text, bool isCorrect, int order)
        {
            Text = text;
            IsCorrect = isCorrect;
            Order = order;
        }
    }
}
