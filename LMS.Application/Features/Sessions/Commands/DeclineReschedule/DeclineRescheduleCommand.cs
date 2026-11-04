using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.DeclineReschedule
{
    public class DeclineRescheduleCommand : IRequest<Result<RescheduleRequestDto>>
    {
        public Guid Id { get; set; }
    }
}
