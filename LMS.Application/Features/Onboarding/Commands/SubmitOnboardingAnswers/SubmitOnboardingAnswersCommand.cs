using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using MediatR;

namespace LMS.Application.Features.Onboarding.Commands.SubmitOnboardingAnswers
{
    public class OnboardingAnswerInput
    {
        public Guid QuestionId { get; set; }
        public List<Guid>? OptionIds { get; set; }
        public decimal? NumericValue { get; set; }
    }

    public class SubmitOnboardingAnswersCommand : IRequest<Result<string>>
    {
        public List<OnboardingAnswerInput>? Answers { get; set; }
    }
}
