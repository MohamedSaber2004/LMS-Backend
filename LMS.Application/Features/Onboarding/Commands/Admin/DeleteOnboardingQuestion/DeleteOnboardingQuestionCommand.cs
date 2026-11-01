using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Onboarding.Commands.Admin.DeleteOnboardingQuestion
{
    public class DeleteOnboardingQuestionCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
