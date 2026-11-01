using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingQuestion
{
    public class OnboardingOptionInput
    {
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public int Order { get; set; }
    }

    public class AddOnboardingQuestionCommand : IRequest<Result<OnboardingQuestionDto>>
    {
        public string Key { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public OnboardingQuestionType Type { get; set; } = OnboardingQuestionType.SingleSelect;

        public int Order { get; set; }

        public decimal? MinValue { get; set; }

        public decimal? MaxValue { get; set; }

        public decimal? Step { get; set; }

        public List<OnboardingOptionInput>? Options { get; set; }
    }
}
