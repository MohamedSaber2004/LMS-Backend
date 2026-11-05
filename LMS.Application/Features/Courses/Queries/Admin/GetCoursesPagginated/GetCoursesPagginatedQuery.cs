using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Courses.Queries.Admin.GetCoursesPagginated
{
    public class GetCoursesPagginatedQuery : IRequest<Result<PagginatedResult<CourseListItemDto>>>
    {
        public GetCoursesPagginatedQuery() { }

        public string? Search { get; set; }

        public CourseLevel? Level { get; set; }

        public bool? IsActive { get; set; }

        public int PageNumber { get; set; } = PagginatedResult<CourseListItemDto>.DefaultPageNumber;

        public int PageSize { get; set; } = PagginatedResult<CourseListItemDto>.DefaultPageSize;
    }
}
