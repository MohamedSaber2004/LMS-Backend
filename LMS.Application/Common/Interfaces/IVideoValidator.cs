using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Interfaces
{
    public interface IVideoValidator
    {
        Task<(bool Uploaded, string Result)> UploadVideo(IFormFile? file, int place);
        Task<(bool Uploaded, string Result)> UploadMultipleVideo(List<IFormFile>? files, int place);

        bool VideoIsExisted(string? fullVideoPath);
        Task<bool> DeleteVideo(string? fileName, int place);
        string GetUniqueFileName(string? fileName);
        bool IsValidVideo(string? videoName, string? placeHolder);
        bool IsValidVideo(IFormFile? file);
    }
}
