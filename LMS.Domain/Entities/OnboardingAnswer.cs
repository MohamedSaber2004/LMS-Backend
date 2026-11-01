using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class OnboardingAnswer : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;

        public Guid OnboardingQuestionId { get; set; }
        public virtual OnboardingQuestion OnboardingQuestion { get; set; } = null!;

        public Guid? OnboardingOptionId { get; set; }
        public virtual OnboardingOption? OnboardingOption { get; set; }

        public decimal? NumericValue { get; set; }
    }
}
