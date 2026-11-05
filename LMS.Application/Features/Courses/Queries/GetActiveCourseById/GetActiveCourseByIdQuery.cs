using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Queries.GetActiveCourseById
{
    public class GetActiveCourseByIdQuery : IRequest<Result<CourseDetailDto>>
    {
        public Guid Id { get; set; }
    }
}
