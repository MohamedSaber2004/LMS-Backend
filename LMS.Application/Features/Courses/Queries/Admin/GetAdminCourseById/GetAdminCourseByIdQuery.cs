using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Queries.Admin.GetAdminCourseById
{
    public class GetAdminCourseByIdQuery : IRequest<Result<CourseAdminDetailDto>>
    {
        public Guid Id { get; set; }
    }
}
