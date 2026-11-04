using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.CreateBooking
{
    public class CreateBookingCommand : IRequest<Result<SessionDto>>
    {
        public Guid InstructorId { get; set; }

        public Guid AvailabilityInstanceId { get; set; }
    }
}
