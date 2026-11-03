using LMS.Application.Common.Models;
using LMS.Application.Features.Availability.DTOs;
using MediatR;

namespace LMS.Application.Features.Availability.Queries.GetInstructorAvailabilityPagginated
{
    public class GetInstructorAvailabilityPagginatedQuery
        : IRequest<Result<PagginatedResult<InstructorAvailabilityRowDto>>>
    {
        public GetInstructorAvailabilityPagginatedQuery() { }

        public Guid? InstructorId { get; set; }

        public DayOfWeek? Day { get; set; }

        public int PageNumber { get; set; } = PagginatedResult<InstructorAvailabilityRowDto>.DefaultPageNumber;

        public int PageSize { get; set; } = PagginatedResult<InstructorAvailabilityRowDto>.DefaultPageSize;
    }
}
