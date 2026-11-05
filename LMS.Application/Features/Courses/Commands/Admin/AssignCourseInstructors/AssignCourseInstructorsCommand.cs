using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.Admin.AssignCourseInstructors
{
    public class AssignCourseInstructorsCommand : IRequest<Result<IReadOnlyCollection<InstructorRefDto>>>
    {
        public Guid Id { get; set; }

        public Guid[] InstructorIds { get; set; } = Array.Empty<Guid>();
    }
}
