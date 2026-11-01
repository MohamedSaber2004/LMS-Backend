using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using MediatR;

namespace LMS.Application.Features.Onboarding.Commands.Admin.UpdateOnboardingQuestion
{
    public class UpdateOnboardingQuestionCommand : IRequest<Result<OnboardingQuestionDto>>
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public decimal? MinValue { get; set; }

        public decimal? MaxValue { get; set; }

        public decimal? Step { get; set; }
    }
}
