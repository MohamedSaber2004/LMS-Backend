using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.EnrollCourse
{
    public class EnrollCourseCommand : IRequest<Result<CourseDetailDto>>
    {
        public Guid CourseId { get; set; }
    }
}
