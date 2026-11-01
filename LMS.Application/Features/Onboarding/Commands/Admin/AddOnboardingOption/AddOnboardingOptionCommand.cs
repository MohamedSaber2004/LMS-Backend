using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using MediatR;

namespace LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingOption
{
    public class AddOnboardingOptionCommand : IRequest<Result<OnboardingOptionDto>>
    {
        public Guid QuestionId { get; set; }

        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}
