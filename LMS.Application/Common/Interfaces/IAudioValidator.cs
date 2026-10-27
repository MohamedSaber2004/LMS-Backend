using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Interfaces
{
    public interface IAudioValidator
    {
        Task<(bool Uploaded, string Result)> UploadAudio(IFormFile? file, int place);
        bool AudioIsExisted(string? fullAudioPath);
        Task<bool> DeleteAudio(string? fileName, int place);
        string GetUniqueFileName(string? fileName);
        bool IsValidAudio(string? audioName, string? placeHolder);
        bool IsValidAudio(IFormFile? file);
    }
}
