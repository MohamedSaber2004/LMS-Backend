using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Services;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Infrastructure.Services.Attachments
{
    public class AudioValidator : IAudioValidator
    {
        private static readonly string[] AllowedExtensions = [".mp3", ".wav", ".ogg", ".m4a", ".aac"];

        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public AudioValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadAudio(IFormFile? file, int place)
        {
            if (!IsValidAudio(file))
                return (false, _localizer[LocalizationKeys.Attachments.InvalidAudioFormat].Value);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(place));
            if (uploaded)
            {
                return (true, $"{place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public bool AudioIsExisted(string? fullAudioPath)
        {
            return _baseFileService.FileExists(fullAudioPath);
        }

        public async Task<bool> DeleteAudio(string? fileName, int place)
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

        public bool IsValidAudio(string? audioName, string? placeHolder)
        {
            return !string.IsNullOrWhiteSpace(audioName) && audioName != placeHolder;
        }

        public bool IsValidAudio(IFormFile? file)
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
