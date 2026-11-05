using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.UpdateLessonProgress
{
    public class UpdateLessonProgressCommand : IRequest<Result<LessonProgressDto>>
    {
        public Guid LessonId { get; set; }

        public int LastWatchedSeconds { get; set; }

        public bool IsCompleted { get; set; }
    }
}
