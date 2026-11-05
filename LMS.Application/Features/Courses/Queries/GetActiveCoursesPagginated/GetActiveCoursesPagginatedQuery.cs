using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Courses.Queries.GetActiveCoursesPagginated
{
    public class GetActiveCoursesPagginatedQuery
        : IRequest<Result<PagginatedResult<CourseListItemDto>>>
    {
        public GetActiveCoursesPagginatedQuery() { }

        public string? Search { get; set; }

        public CourseLevel? Level { get; set; }

        public Guid? InstructorId { get; set; }

        public int PageNumber { get; set; } = PagginatedResult<CourseListItemDto>.DefaultPageNumber;

        public int PageSize { get; set; } = PagginatedResult<CourseListItemDto>.DefaultPageSize;
    }
}
