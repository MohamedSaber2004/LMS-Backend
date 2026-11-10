using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class CourseEnrollment : BaseEntity<Guid>
    {
        public Guid CourseId { get; private set; }
        public virtual Course Course { get; private set; } = null!;

        public Guid LearnerId { get; private set; }
        public virtual Learner Learner { get; private set; } = null!;

        public decimal PricePaid { get; private set; }

        public static CourseEnrollment Create(Guid courseId, Guid learnerId, decimal pricePaid)
        {
            return new CourseEnrollment
            {
                CourseId = courseId,
                LearnerId = learnerId,
                PricePaid = pricePaid
            };
        }
    }
}
