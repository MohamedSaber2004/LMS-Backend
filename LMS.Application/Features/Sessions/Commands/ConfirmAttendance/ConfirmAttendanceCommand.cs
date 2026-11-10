using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.ConfirmAttendance
{
    public class ConfirmAttendanceCommand : IRequest<Result<SessionDto>>
    {
        public Guid SessionId { get; set; }
    }
}

