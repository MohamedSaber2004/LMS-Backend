using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.Admin.UpdateQuizQuestion
{
    public class UpdateQuizQuestionCommand : IRequest<Result<QuizQuestionAdminDto>>
    {
        public Guid Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public int Order { get; set; }

        public bool IsActive { get; set; } = true;

        public List<QuizOptionInput>? Options { get; set; }
    }
}
