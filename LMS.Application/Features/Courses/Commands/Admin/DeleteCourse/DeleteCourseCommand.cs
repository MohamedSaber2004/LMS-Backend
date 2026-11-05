using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.Admin.DeleteCourse
{
    public class DeleteCourseCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
