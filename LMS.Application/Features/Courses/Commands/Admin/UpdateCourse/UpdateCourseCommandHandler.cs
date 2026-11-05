using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.UpdateCourse
{
    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Result<CourseListItemDto>>
    {
        private const int CoverFilePlace = (int)MediaKind.Image;

        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImageValidator _imageValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateCourseCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IImageValidator imageValidator,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _imageValidator = imageValidator;
            _localizer = localizer;
        }

        public async Task<Result<CourseListItemDto>> Handle(
            UpdateCourseCommand request,
            CancellationToken cancellationToken)
        {
            var course = await _context.Courses
                .Include(item => item.Lessons)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.CourseNotFound);

            var previousCover = course.CoverUrl;

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

            var coverSource = uploadedCover ?? ResolveCover(request.CoverUrl, previousCover);

            try
            {
                course.Update(
                    request.Title.Trim(),
                    request.Description.Trim(),
                    coverSource,
                    request.Level,
                    request.Order);

                course.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

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

            if (coverSource != previousCover)
            {
                await DeleteReplacedCoverAsync(previousCover);
            }

            return Result<CourseListItemDto>.Success(
                CoursesMapper.ToListItemDto(course, course.Lessons.Count(item => item.IsActive)),
                _localizer[LocalizationKeys.Courses.CourseUpdatedSuccess].Value);
        }

        private static string? ResolveCover(string? requested, string? current)
        {
            if (requested is null)
            {
                return current;
            }

            return string.IsNullOrWhiteSpace(requested) ? null : requested.Trim();
        }

        private async Task DeleteReplacedCoverAsync(string? previous)
        {
            if (string.IsNullOrWhiteSpace(previous)
                || !previous.StartsWith($"{CoverFilePlace}_", StringComparison.Ordinal))
            {
                return;
            }

            await _imageValidator.DeleteImage(previous, CoverFilePlace);
        }
    }
}
