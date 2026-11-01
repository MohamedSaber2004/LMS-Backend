using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.DTOs;
using MediatR;

namespace LMS.Application.Features.Onboarding.Queries.GetMyOnboardingAnswers
{
    public class GetMyOnboardingAnswersQuery : IRequest<Result<IReadOnlyCollection<OnboardingAnswerGroupDto>>>
    {
        public GetMyOnboardingAnswersQuery() { }
    }
}
