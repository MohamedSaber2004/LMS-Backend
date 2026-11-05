using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;

namespace LMS.Application.Features.Courses.Queries.GetLessonContent
{
    public class GetLessonContentQuery : IRequest<Result<LessonContentDto>>
    {
        public Guid LessonId { get; set; }
    }
}
