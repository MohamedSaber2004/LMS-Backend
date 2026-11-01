using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class OnboardingQuestion : BaseEntity<Guid>
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public OnboardingQuestionType Type { get; set; } = OnboardingQuestionType.SingleSelect;
        public int Order { get; set; }

        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? Step { get; set; }

        public virtual ICollection<OnboardingOption> Options { get; set; } = new List<OnboardingOption>();
    }
}
