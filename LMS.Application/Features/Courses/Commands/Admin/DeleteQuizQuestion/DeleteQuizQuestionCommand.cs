using LMS.Application.Common.Models;
using MediatR;

namespace LMS.Application.Features.Courses.Commands.Admin.DeleteQuizQuestion
{
    public class DeleteQuizQuestionCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
