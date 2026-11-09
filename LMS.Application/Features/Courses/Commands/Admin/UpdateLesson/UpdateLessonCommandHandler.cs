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

namespace LMS.Application.Features.Courses.Commands.Admin.UpdateLesson
{
    public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, Result<LessonAdminDto>>
    {
        private const int VideoFilePlace = (int)MediaKind.Video;
        private const int StudyBookFilePlace = (int)MediaKind.Document;

        private readonly ILmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IVideoValidator _videoValidator;
        private readonly IFileValidator _fileValidator;
        private readonly IStringLocalizer<Messages> _localizer;

        public UpdateLessonCommandHandler(
            ILmsDbContext context,
            ICurrentUserService currentUserService,
            IVideoValidator videoValidator,
            IFileValidator fileValidator,
            IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _currentUserService = currentUserService;
            _videoValidator = videoValidator;
            _fileValidator = fileValidator;
            _localizer = localizer;
        }

        public async Task<Result<LessonAdminDto>> Handle(
            UpdateLessonCommand request,
            CancellationToken cancellationToken)
        {
            var lesson = await _context.Lessons
                .Include(item => item.QuizQuestions)
                .ThenInclude(question => question.Options)
                .Include(item => item.Session)
                .ThenInclude(session => session!.LiveRoom)
                .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(LocalizationKeys.Courses.LessonNotFound);

            var previousVideo = lesson.VideoUrl;
            var previousStudyBook = lesson.StudyBookUrl;

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

                var videoSource = uploadedVideo ?? ResolveVideo(request.VideoUrl, previousVideo);
                var studyBookSource = uploadedStudyBook ?? ResolveStudyBook(request.StudyBookUrl, previousStudyBook);

                lesson.Update(
                    request.Title.Trim(),
                    videoSource,
                    studyBookSource,
                    request.Order,
                    request.IsFreeDemo);

                lesson.SetActiveState(request.IsActive, _currentUserService.UserId.ToString());

                await _context.SaveChangesAsync(cancellationToken);

                await DeleteReplacedFileAsync(previousVideo, videoSource, VideoFilePlace);
                await DeleteReplacedFileAsync(previousStudyBook, studyBookSource, StudyBookFilePlace);

                return Result<LessonAdminDto>.Success(
                    CoursesMapper.ToAdminLessonDto(lesson),
                    _localizer[LocalizationKeys.Courses.LessonUpdatedSuccess].Value);
            }
            catch
            {
                await DeleteUploadedFilesAsync(uploadedFiles);
                throw;
            }
        }

        private static string ResolveVideo(string? requested, string? current)
        {
            return string.IsNullOrWhiteSpace(requested) ? current! : requested.Trim();
        }

        private static string? ResolveStudyBook(string? requested, string? current)
        {
            if (requested is null)
            {
                return current;
            }

            return string.IsNullOrWhiteSpace(requested) ? null : requested.Trim();
        }

        private async Task DeleteReplacedFileAsync(string? previous, string? current, int place)
        {
            if (string.IsNullOrWhiteSpace(previous) || previous == current)
            {
                return;
            }

            if (!previous.StartsWith($"{place}_", StringComparison.Ordinal))
            {
                return;
            }

            if (place == VideoFilePlace)
            {
                await _videoValidator.DeleteVideo(previous, place);
                return;
            }

            await _fileValidator.DeleteFile(previous, place);
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
