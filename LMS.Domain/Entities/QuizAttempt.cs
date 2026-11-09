using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class QuizAttempt : BaseEntity<Guid>
    {
        public Guid? LessonId { get; private set; }
        public virtual Lesson? Lesson { get; private set; }

        public Guid? CourseId { get; private set; }
        public virtual Course? Course { get; private set; }

        public Guid LearnerId { get; private set; }
        public virtual Learner Learner { get; private set; } = null!;

        public int Score { get; private set; }

        public int TotalQuestions { get; private set; }

        public virtual ICollection<QuizAttemptAnswer> Answers { get; private set; } = new List<QuizAttemptAnswer>();

        public static QuizAttempt Create(Guid lessonId, Guid learnerId, int score, int totalQuestions)
        {
            return new QuizAttempt
            {
                LessonId = lessonId,
                LearnerId = learnerId,
                Score = score,
                TotalQuestions = totalQuestions
            };
        }

        public static QuizAttempt CreateForCourse(Guid courseId, Guid learnerId, int score, int totalQuestions)
        {
            return new QuizAttempt
            {
                CourseId = courseId,
                LearnerId = learnerId,
                Score = score,
                TotalQuestions = totalQuestions
            };
        }

        public void AddAnswer(Guid questionId, Guid selectedOptionId)
        {
            Answers.Add(QuizAttemptAnswer.Create(Id, questionId, selectedOptionId));
        }
    }
}
