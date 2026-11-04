using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.ApproveReschedule
{
    public class ApproveRescheduleCommand : IRequest<Result<SessionDto>>
    {
        public Guid Id { get; set; }
    }
}
