using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Services;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Infrastructure.Services.Attachments
{
    public class FileValidator : IFileValidator
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip", ".rar"];

        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public FileValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadFile(IFormFile? file, int place)
        {
            if (!IsValidFile(file))
                return (false, _localizer[LocalizationKeys.Attachments.InvalidFileFormat].Value);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(place));
            if (uploaded)
            {
                return (true, $"{place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public bool FileIsExisted(string? fullFilePath)
        {
            return _baseFileService.FileExists(fullFilePath);
        }

        public async Task<bool> DeleteFile(string? fileName, int place)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            fileName = StripPlacePrefix(fileName);
            return await _baseFileService.DeleteFileAsync(fileName, GetFolderPath(place));
        }

        public string GetUniqueFileName(string? fileName)
        {
            return _baseFileService.GetUniqueFileName(fileName);
        }

        public bool IsValidFile(string? fileName, string? placeHolder)
        {
            return !string.IsNullOrWhiteSpace(fileName) && fileName != placeHolder;
        }

        public bool IsValidFile(IFormFile? file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(file.FileName))
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public async Task<(bool Success, string Result)> DownloadFile(int filePlace, string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return (false, _localizer[LocalizationKeys.Attachments.FileNotFound].Value);

            fileName = StripPlacePrefix(fileName);
            return await _baseFileService.DownloadFileAsync(GetFolderPath(filePlace), fileName);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleFile(List<IFormFile>? files, int place)
        {
            if (files == null || files.Count == 0)
                return (false, _localizer[LocalizationKeys.Attachments.NoMediaProvided].Value);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadFile(file, place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (results.Count == 0)
                return (false, _localizer[LocalizationKeys.Attachments.UploadFailed].Value);

            return (true, string.Join(",", results));
        }

        private static string StripPlacePrefix(string fileName)
        {
            if (fileName.Contains('_'))
            {
                var parts = fileName.Split('_');
                if (int.TryParse(parts[0], out _))
                {
                    return string.Join("_", parts.Skip(1));
                }
            }
            return fileName;
        }

        private static string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place) ?? string.Empty;
        }
    }
}
