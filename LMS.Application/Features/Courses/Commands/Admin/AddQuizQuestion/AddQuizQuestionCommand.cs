using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.Admin.AddQuizQuestion
{
    public class AddQuizQuestionCommand : IRequest<Result<QuizQuestionAdminDto>>
    {
        public Guid LessonId { get; set; }

        public string Text { get; set; } = string.Empty;

        public int Order { get; set; }

        public List<QuizOptionInput>? Options { get; set; }
    }
}
