using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.ApproveInstructorApplication
{
    public class ApproveInstructorApplicationCommand : IRequest<Result<string>>
    {
        public ApproveInstructorApplicationCommand() { }

        public ApproveInstructorApplicationCommand(Guid applicationId, decimal hourlyRate, string? reviewNotes = null)
        {
            ApplicationId = applicationId;
            HourlyRate = hourlyRate;
            ReviewNotes = reviewNotes;
        }

        public Guid ApplicationId { get; set; }
        public decimal HourlyRate { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
