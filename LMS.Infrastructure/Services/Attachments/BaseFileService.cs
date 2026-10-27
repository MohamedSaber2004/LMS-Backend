using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Services.Attachments
{
    public class BaseFileService : IBaseFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ILogger<BaseFileService> _logger;

        private string WebRootPath => _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

        public BaseFileService(
            IWebHostEnvironment webHostEnvironment,
            IStringLocalizer<Messages> localizer,
            ILogger<BaseFileService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<(bool Uploaded, string Result)> UploadFileAsync(IFormFile? file, string folderPath)
        {
            if (file == null || file.Length == 0)
                return (false, _localizer[LocalizationKeys.Attachments.FileEmpty].Value);

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                _logger.LogWarning("Upload file failed: folderPath is null or empty. Please check the requested place parameter or UploadPaths configuration.");
                return (false, _localizer[LocalizationKeys.Attachments.UploadFailed].Value);
            }

            try
            {
                string sanitizedFolder = folderPath.TrimStart('/', '\\');
                string uploadsFolder = Path.Combine(WebRootPath, sanitizedFolder);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = GetUniqueFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return (true, Path.Combine(sanitizedFolder, uniqueFileName).Replace("\\", "/"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload file failed due to exception: {Message}", ex.Message);
                return (false, _localizer[LocalizationKeys.Attachments.UploadFailed].Value);
            }
        }

        public bool FileExists(string? fullFilePath)
        {
            if (string.IsNullOrWhiteSpace(fullFilePath))
                return false;

            string filePath = Path.Combine(WebRootPath, fullFilePath.TrimStart('/', '\\'));
            return File.Exists(filePath);
        }

        public Task<bool> DeleteFileAsync(string fileName, string folderPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(folderPath))
                    return Task.FromResult(false);

                string safeFileName = Path.GetFileName(fileName);
                string filePath = Path.Combine(WebRootPath, folderPath.TrimStart('/', '\\'), safeFileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public string GetUniqueFileName(string? fileName)
        {
            var extension = string.IsNullOrEmpty(fileName) ? string.Empty : Path.GetExtension(fileName);
            return Guid.NewGuid().ToString() + extension;
        }

        public Task<(bool Success, string Result)> DownloadFileAsync(string folderPath, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(folderPath) && !string.IsNullOrWhiteSpace(fileName))
            {
                string safeFileName = Path.GetFileName(fileName);
                string filePath = Path.Combine(WebRootPath, folderPath.TrimStart('/', '\\'), safeFileName);

                if (File.Exists(filePath))
                {
                    return Task.FromResult((true, filePath));
                }
            }

            return Task.FromResult((false, _localizer[LocalizationKeys.Attachments.FileNotFound].Value));
        }

        public string GetPhysicalPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return string.Empty;

            return Path.Combine(WebRootPath, relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
