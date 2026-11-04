using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Queries.GetMyAvailabilityCalendar
{
    public class GetMyAvailabilityCalendarQuery : IRequest<Result<AvailabilityCalendarDto>>
    {
        public GetMyAvailabilityCalendarQuery() { }

        public DateOnly From { get; set; }

        public DateOnly To { get; set; }
    }
}
