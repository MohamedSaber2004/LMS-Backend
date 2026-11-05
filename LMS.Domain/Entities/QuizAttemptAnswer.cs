using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class QuizAttemptAnswer : BaseEntity<Guid>
    {
        public Guid QuizAttemptId { get; private set; }
        public virtual QuizAttempt QuizAttempt { get; private set; } = null!;

        public Guid QuestionId { get; private set; }

        public Guid SelectedOptionId { get; private set; }

        public static QuizAttemptAnswer Create(Guid quizAttemptId, Guid questionId, Guid selectedOptionId)
        {
            return new QuizAttemptAnswer
            {
                QuizAttemptId = quizAttemptId,
                QuestionId = questionId,
                SelectedOptionId = selectedOptionId
            };
        }
    }
}
