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
        public decimal? HourlyRate { get; set; }
        public decimal? CommissionRate { get; set; }
        public bool? IsNativeSpeaker { get; set; }
        public List<string>? Languages { get; set; }
    }
}
