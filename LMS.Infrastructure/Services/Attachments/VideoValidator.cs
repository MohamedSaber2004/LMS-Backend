using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Services;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Services.Attachments
{
    public class VideoValidator : IVideoValidator
    {
        private static readonly string[] AllowedExtensions = [".mp4", ".avi", ".mkv", ".mov", ".wmv"];

        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public VideoValidator(
            IBaseFileService baseFileService,
            IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadVideo(IFormFile? file, int place)
        {
            if (!IsValidVideo(file))
                return (false, _localizer[LocalizationKeys.Attachments.InvalidVideoFormat].Value);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(place));
            if (uploaded)
            {
                return (true, $"{place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleVideo(List<IFormFile>? files, int place)
        {
            if (files == null || files.Count == 0)
                return (false, _localizer[LocalizationKeys.Attachments.NoMediaProvided].Value);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadVideo(file, place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (results.Count == 0)
                return (false, _localizer[LocalizationKeys.Attachments.UploadFailed].Value);

            return (true, string.Join(",", results));
        }

        public bool VideoIsExisted(string? fullVideoPath)
        {
            return _baseFileService.FileExists(fullVideoPath);
        }

        public async Task<bool> DeleteVideo(string? fileName, int place)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            if (fileName.Contains('_'))
            {
                var parts = fileName.Split('_');
                if (int.TryParse(parts[0], out _))
                {
                    fileName = string.Join("_", parts.Skip(1));
                }
            }
            return await _baseFileService.DeleteFileAsync(fileName, GetFolderPath(place));
        }

        public string GetUniqueFileName(string? fileName)
        {
            return _baseFileService.GetUniqueFileName(fileName);
        }

        public bool IsValidVideo(string? videoName, string? placeHolder)
        {
            return !string.IsNullOrWhiteSpace(videoName) && videoName != placeHolder;
        }

        public bool IsValidVideo(IFormFile? file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(file.FileName))
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        private static string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place) ?? string.Empty;
        }
    }
}
