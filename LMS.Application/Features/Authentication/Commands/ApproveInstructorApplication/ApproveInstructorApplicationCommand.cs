using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.ApproveInstructorApplication
{
    public class ApproveInstructorApplicationCommand : IRequest<Result<string>>
    {
        public ApproveInstructorApplicationCommand() { }

        public ApproveInstructorApplicationCommand(Guid applicationId, string? reviewNotes = null)
        {
            ApplicationId = applicationId;
            ReviewNotes = reviewNotes;
        }

        public Guid ApplicationId { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
