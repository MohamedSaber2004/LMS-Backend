using LMS.Application.Features.Availability.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Features.Availability
{
    public static class AvailabilityMapper
    {
        public static AvailabilitySlotDto ToSlotDto(InstructorAvailabilitySlot slot) => new()
        {
            Id = slot.Id,
            InstructorId = slot.InstructorId,
            Day = (int)slot.Day,
            DayName = slot.Day.ToString(),
            StartTime = slot.StartTime,
            EndTime = slot.EndTime
        };

        public static InstructorAvailabilityRowDto ToRowDto(InstructorAvailabilitySlot slot, string instructorFullName) => new()
        {
            Id = slot.Id,
            InstructorId = slot.InstructorId,
            InstructorFullName = instructorFullName,
            Specialization = slot.Instructor?.Specialization ?? string.Empty,
            Day = (int)slot.Day,
            DayName = slot.Day.ToString(),
            StartTime = slot.StartTime,
            EndTime = slot.EndTime
        };
    }
}
