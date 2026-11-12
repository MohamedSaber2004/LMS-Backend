using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Sessions.Commands.CreateBooking
{
    public class CreateBookingCommand : IRequest<Result<SessionDto>>
    {
        public Guid InstructorId { get; set; }

        public DateOnly Date { get; set; }

        public TimeSpan? StartTime { get; set; }

        public AcademicSessionDuration DurationMinutes { get; set; } = AcademicSessionDuration.Session50Min;
    }
}
