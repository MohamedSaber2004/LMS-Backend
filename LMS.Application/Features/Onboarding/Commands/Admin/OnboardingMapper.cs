using LMS.Application.Features.Onboarding.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Features.Onboarding.Commands.Admin
{
    public static class OnboardingMapper
    {
        public static OnboardingQuestionDto ToDto(OnboardingQuestion question, bool onlyActiveOptions = false)
        {
            var options = question.Options
                .Where(option => !onlyActiveOptions || option.IsActive)
                .OrderBy(option => option.Order)
                .Select(option => new OnboardingOptionDto
                {
                    Id = option.Id,
                    Label = option.Label,
                    Value = option.Value,
                    Order = option.Order,
                    IsActive = option.IsActive
                })
                .ToArray();

            return new OnboardingQuestionDto
            {
                Id = question.Id,
                Key = question.Key,
                Title = question.Title,
                Type = question.Type,
                Order = question.Order,
                MinValue = question.MinValue,
                MaxValue = question.MaxValue,
                Step = question.Step,
                IsActive = question.IsActive,
                Options = options
            };
        }
    }
}
