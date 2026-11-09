using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.RequestReschedule
{
    public class RequestRescheduleCommand : IRequest<Result<RescheduleRequestDto>>
    {
        public Guid Id { get; set; }

        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }
    }
}
