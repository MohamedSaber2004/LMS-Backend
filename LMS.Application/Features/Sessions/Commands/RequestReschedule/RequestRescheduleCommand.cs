using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.RequestReschedule
{
    /// <summary>
    /// Proposes a new window for a session. Nothing on the session moves until the other side approves,
    /// so a request is a record, never an edit.
    /// </summary>
    public class RequestRescheduleCommand : IRequest<Result<RescheduleRequestDto>>
    {
        public Guid Id { get; set; }

        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }
    }
}
