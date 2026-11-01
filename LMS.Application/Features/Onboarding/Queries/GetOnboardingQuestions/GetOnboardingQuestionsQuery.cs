using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using MediatR;

namespace LMS.Application.Features.Onboarding.Queries.GetOnboardingQuestions
{
    public class GetOnboardingQuestionsQuery : IRequest<Result<IReadOnlyCollection<OnboardingQuestionDto>>>
    {
    }
}
