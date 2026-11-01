namespace LMS.Application.Features.Onboarding.DTOs
{
    public class OnboardingAnswerDto
    {
        public OnboardingAnswerDto() { }

        public Guid Id { get; set; }
        public Guid? OptionId { get; set; }
        public string? OptionLabel { get; set; }
        public string? OptionValue { get; set; }
        public decimal? NumericValue { get; set; }
    }

    public class OnboardingAnswerGroupDto
    {
        public OnboardingAnswerGroupDto() { }

        public Guid QuestionId { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public IReadOnlyCollection<OnboardingAnswerDto> Answers { get; set; } = Array.Empty<OnboardingAnswerDto>();
    }
}
