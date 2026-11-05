using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.SubmitQuiz
{
    public class SubmitQuizAnswerInput
    {
        public Guid QuestionId { get; set; }

        public Guid SelectedOptionId { get; set; }
    }

    public class SubmitQuizCommand : IRequest<Result<QuizResultDto>>
    {
        public Guid LessonId { get; set; }

        public List<SubmitQuizAnswerInput>? Answers { get; set; }
    }
}
