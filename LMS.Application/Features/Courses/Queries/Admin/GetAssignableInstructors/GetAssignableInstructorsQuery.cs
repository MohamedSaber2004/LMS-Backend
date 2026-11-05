using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Queries.Admin.GetAssignableInstructors
{
    public class GetAssignableInstructorsQuery : IRequest<Result<PagginatedResult<InstructorRefDto>>>
    {
        public GetAssignableInstructorsQuery() { }

        public string? Search { get; set; }

        public int PageNumber { get; set; } = PagginatedResult<InstructorRefDto>.DefaultPageNumber;

        public int PageSize { get; set; } = PagginatedResult<InstructorRefDto>.DefaultPageSize;
    }
}
