using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using MediatR;

namespace LMS.Application.Features.Onboarding.Queries.GetActiveOnboardingQuestions
{
    public class GetActiveOnboardingQuestionsQuery
        : IRequest<Result<IReadOnlyCollection<OnboardingQuestionDto>>>
    {
    }
}
