using LMS.Application.Common.Models;
using LMS.Application.Features.Sessions.DTOs;
using MediatR;

namespace LMS.Application.Features.Sessions.Queries.GetBookableSlots
{
    public class GetBookableSlotsQuery : IRequest<Result<IReadOnlyCollection<BookableSlotDto>>>
    {
        public GetBookableSlotsQuery() { }

        public Guid InstructorId { get; set; }

        public DateOnly From { get; set; }

        public DateOnly To { get; set; }

        public string? TimeZoneId { get; set; }
    }
}
