using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.AddCourse
{
    public class AddCourseCommandHandler : IRequestHandler<AddCourseCommand, Result<CourseListItemDto>>
    {
        private const int CoverFilePlace = (int)MediaKind.Image;

        private readonly ILmsDbContext _context;
        private readonly IImageValidator _imageValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddCourseCommandHandler(
            ILmsDbContext context,
            IImageValidator imageValidator,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _imageValidator = imageValidator;
            _localizer = localizer;
        }

        public async Task<Result<CourseListItemDto>> Handle(
            AddCourseCommand request,
            CancellationToken cancellationToken)
        {
            string? uploadedCover = null;

            if (request.CoverFile is not null)
            {
                var (uploaded, token) = await _imageValidator.UploadImage(request.CoverFile, CoverFilePlace);
                if (!uploaded)
                {
                    return Result<CourseListItemDto>.Failure(token, StatusCodes.Status400BadRequest);
                }

                uploadedCover = token;
            }

            var course = Course.Create(
                request.Title.Trim(),
                request.Description.Trim(),
                uploadedCover ?? Normalize(request.CoverUrl),
                request.Level,
                request.Order,
                request.Price,
                request.Type);

            try
            {
                await _context.Courses.AddAsync(course, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                if (uploadedCover is not null)
                {
                    await _imageValidator.DeleteImage(uploadedCover, CoverFilePlace);
                }

                throw;
            }

            return Result<CourseListItemDto>.Success(
                CoursesMapper.ToListItemDto(course, 0),
                _localizer[LocalizationKeys.Courses.CourseCreatedSuccess].Value,
                StatusCodes.Status201Created);
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
