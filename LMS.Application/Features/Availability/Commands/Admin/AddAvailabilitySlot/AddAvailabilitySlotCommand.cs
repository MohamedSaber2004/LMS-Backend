using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Commands.Admin.AddAvailabilitySlot
{
    public class AddAvailabilitySlotCommand : IRequest<Result<AvailabilitySlotDto>>
    {
        public Guid InstructorId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
