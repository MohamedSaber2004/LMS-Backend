using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Courses.Commands.Admin.AddCourse
{
    public class AddCourseCommand : IRequest<Result<CourseListItemDto>>
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public IFormFile? CoverFile { get; set; }

        public string? CoverUrl { get; set; }

        public CourseLevel Level { get; set; }

        public CourseType Type { get; set; } = CourseType.Recorded;

        public int Order { get; set; }

        public decimal Price { get; set; }
    }
}
