using LMS.Domain.Enums;

namespace LMS.Application.Features.Onboarding.DTOs
{
    public class OnboardingOptionDto
    {
        public OnboardingOptionDto() { }

        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }

    public class OnboardingQuestionDto
    {
        public OnboardingQuestionDto() { }

        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public OnboardingQuestionType Type { get; set; }
        public int Order { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? Step { get; set; }
        public bool IsActive { get; set; }
        public IReadOnlyCollection<OnboardingOptionDto> Options { get; set; } = Array.Empty<OnboardingOptionDto>();
    }
}
