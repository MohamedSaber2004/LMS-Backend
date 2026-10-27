using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Services;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Services.Attachments
{
    public class ImageValidator : IImageValidator
    {
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

        private const long MaxRemoteImageBytes = 5 * 1024 * 1024;

        private readonly IBaseFileService _baseFileService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<Messages> _localizer;
        private readonly ILogger<ImageValidator> _logger;

        public ImageValidator(
            IBaseFileService baseFileService,
            IHttpClientFactory httpClientFactory,
            IStringLocalizer<Messages> localizer,
            ILogger<ImageValidator> logger)
        {
            _baseFileService = baseFileService;
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<(bool Uploaded, string Result)> UploadImage(IFormFile? file, int place)
        {
            if (!IsValidImage(file))
                return (false, _localizer[LocalizationKeys.Attachments.InvalidImageFormat].Value);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(place));
            if (uploaded)
            {
                return (true, $"{place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleImage(List<IFormFile>? files, int place)
        {
            if (files == null || files.Count == 0)
                return (false, _localizer[LocalizationKeys.Attachments.NoMediaProvided].Value);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadImage(file, place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (results.Count == 0)
                return (false, _localizer[LocalizationKeys.Attachments.UploadFailed].Value);

            return (true, string.Join(",", results));
        }

        public bool ImageIsExisted(string? fullImagePath)
        {
            return _baseFileService.FileExists(fullImagePath);
        }

        public async Task<bool> DeleteImage(string? fileName, int place)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("default-avatar", StringComparison.OrdinalIgnoreCase))
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

        public bool IsValidImage(IFormFile? file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(file.FileName))
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public bool IsValidImage(string? imageName, string? placeHolder)
        {
            return !string.IsNullOrWhiteSpace(imageName) && imageName != placeHolder;
        }

        public async Task<IFormFile?> ConvertImageToFormFile(string? imageUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) || !Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                return null;

            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                return null;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                using var response = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                if (response.Content.Headers.ContentLength is long declaredLength && declaredLength > MaxRemoteImageBytes)
                    return null;

                var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                if (content.Length == 0 || content.Length > MaxRemoteImageBytes)
                    return null;

                return new FormFile(new MemoryStream(content), 0, content.Length, "file", BuildFileName(uri, response.Content.Headers.ContentType?.MediaType))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream"
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to download image from {Host}: {Reason}", uri.Host, ex.Message);
                return null;
            }
        }

        private static string BuildFileName(Uri uri, string? mediaType)
        {
            var fromUrl = Path.GetFileName(uri.LocalPath);
            var existingExtension = Path.GetExtension(fromUrl);

            if (AllowedExtensions.Contains(existingExtension, StringComparer.OrdinalIgnoreCase))
                return fromUrl;

            var extension = MapMediaTypeToExtension(mediaType);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fromUrl);

            return string.IsNullOrWhiteSpace(nameWithoutExtension)
                ? $"downloaded_image{extension}"
                : $"{nameWithoutExtension}{extension}";
        }

        private static string MapMediaTypeToExtension(string? mediaType) => mediaType?.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/bmp" or "image/x-ms-bmp" => ".bmp",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        private static string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place) ?? string.Empty;
        }
    }
}
