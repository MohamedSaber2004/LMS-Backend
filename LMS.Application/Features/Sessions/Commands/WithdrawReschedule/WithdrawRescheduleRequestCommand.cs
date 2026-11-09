using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.WithdrawReschedule
{
    public class WithdrawRescheduleRequestCommand : IRequest<Result<RescheduleRequestDto>>
    {
        public Guid Id { get; set; }
    }
}

