using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses.Commands.Admin.AddLesson
{
    public class AddLessonCommandHandler : IRequestHandler<AddLessonCommand, Result<LessonAdminDto>>
    {
        private const int VideoFilePlace = (int)MediaKind.Video;
        private const int StudyBookFilePlace = (int)MediaKind.Document;

        private readonly ILmsDbContext _context;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public AddLessonCommandHandler(
            ILmsDbContext context,
            IVideoValidator videoValidator,
            IFileValidator fileValidator,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _videoValidator = videoValidator;
            _fileValidator = fileValidator;
            _localizer = localizer;
        }

        public async Task<Result<LessonAdminDto>> Handle(
            AddLessonCommand request,
            CancellationToken cancellationToken)
        {
            var uploadedFiles = new List<(string Token, int Place)>();

            try
            {
                string? uploadedVideo = null;
                string? uploadedStudyBook = null;

                if (request.VideoFile is not null)
                {
                    var (uploaded, token) = await _videoValidator.UploadVideo(request.VideoFile, VideoFilePlace);
                    if (!uploaded)
                    {
                        return Result<LessonAdminDto>.Failure(token, StatusCodes.Status400BadRequest);
                    }

                    uploadedFiles.Add((token, VideoFilePlace));
                    uploadedVideo = token;
                }

                if (request.StudyBookFile is not null)
                {
                    var (uploaded, token) = await _fileValidator.UploadFile(request.StudyBookFile, StudyBookFilePlace);
                    if (!uploaded)
                    {
                        await DeleteUploadedFilesAsync(uploadedFiles);
                        return Result<LessonAdminDto>.Failure(token, StatusCodes.Status400BadRequest);
                    }

                    uploadedFiles.Add((token, StudyBookFilePlace));
                    uploadedStudyBook = token;
                }

                var lesson = Lesson.Create(
                    request.CourseId,
                    request.Title.Trim(),
                    uploadedVideo ?? request.VideoUrl!.Trim(),
                    uploadedStudyBook ?? Normalize(request.StudyBookUrl),
                    request.Order,
                    request.IsFreeDemo);

                await _context.Lessons.AddAsync(lesson, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Result<LessonAdminDto>.Success(
                    CoursesMapper.ToAdminLessonDto(lesson),
                    _localizer[LocalizationKeys.Courses.LessonCreatedSuccess].Value,
                    StatusCodes.Status201Created);
            }
            catch
            {
                await DeleteUploadedFilesAsync(uploadedFiles);
                throw;
            }
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private async Task DeleteUploadedFilesAsync(List<(string Token, int Place)> uploadedFiles)
        {
            foreach (var (token, place) in uploadedFiles)
            {
                if (place == VideoFilePlace)
                {
                    await _videoValidator.DeleteVideo(token, place);
                    continue;
                }

                await _fileValidator.DeleteFile(token, place);
            }
        }
    }
}
