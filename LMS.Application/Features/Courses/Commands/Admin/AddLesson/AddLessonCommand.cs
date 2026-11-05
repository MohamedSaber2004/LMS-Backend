using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Courses.Commands.Admin.AddLesson
{
    public class AddLessonCommand : IRequest<Result<LessonAdminDto>>
    {
        public Guid CourseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public IFormFile? VideoFile { get; set; }

        public string? VideoUrl { get; set; }

        public IFormFile? StudyBookFile { get; set; }

        public string? StudyBookUrl { get; set; }

        public int Order { get; set; }

        public bool IsFreeDemo { get; set; }
    }
}
