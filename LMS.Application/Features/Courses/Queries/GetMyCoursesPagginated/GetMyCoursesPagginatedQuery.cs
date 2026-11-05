using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Queries.GetMyCoursesPagginated
{
    public class GetMyCoursesPagginatedQuery : IRequest<Result<PagginatedResult<MyCourseDto>>>
    {
        public GetMyCoursesPagginatedQuery() { }

        public int PageNumber { get; set; } = PagginatedResult<MyCourseDto>.DefaultPageNumber;

        public int PageSize { get; set; } = PagginatedResult<MyCourseDto>.DefaultPageSize;
    }
}
