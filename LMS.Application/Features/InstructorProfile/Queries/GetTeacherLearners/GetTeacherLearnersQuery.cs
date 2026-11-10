using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using MediatR;

namespace LMS.Application.Features.InstructorProfile.Queries.GetTeacherLearners
{
    public class GetTeacherLearnersQuery : IRequest<Result<PagginatedResult<TeacherLearnerItemDto>>>
    {
        public string? Search { get; set; }
        public int PageNumber { get; set; } = PagginatedResult<TeacherLearnerItemDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<TeacherLearnerItemDto>.DefaultPageSize;
    }
}

