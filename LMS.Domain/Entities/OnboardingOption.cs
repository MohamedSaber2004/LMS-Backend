using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class OnboardingOption : BaseEntity<Guid>
    {
        public Guid OnboardingQuestionId { get; set; }
        public virtual OnboardingQuestion OnboardingQuestion { get; set; } = null!;

        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
