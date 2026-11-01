using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Onboarding.Commands.Admin.DeleteOnboardingOption
{
    public class DeleteOnboardingOptionCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
