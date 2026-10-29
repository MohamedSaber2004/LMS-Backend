using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.RejectInstructorApplication
{
    public class RejectInstructorApplicationCommand : IRequest<Result<string>>
    {
        public RejectInstructorApplicationCommand() { }

        public RejectInstructorApplicationCommand(Guid applicationId, string reviewNotes)
        {
            ApplicationId = applicationId;
            ReviewNotes = reviewNotes;
        }

        public Guid ApplicationId { get; set; }
        public string ReviewNotes { get; set; } = string.Empty;
    }
}
