using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class Learner : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public virtual ApplicationUser User { get; private set; } = null!;

        public static Learner Create(Guid userId)
        {
            return new Learner
            {
                UserId = userId
            };
        }
    }
}
