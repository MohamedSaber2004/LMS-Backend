using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.Admin.DeleteLesson
{
    public class DeleteLessonCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
