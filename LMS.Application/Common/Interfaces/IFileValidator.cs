using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Interfaces
{
    public interface IFileValidator
    {
        Task<(bool Uploaded, string Result)> UploadFile(IFormFile? file, int place);
        bool FileIsExisted(string? fullFilePath);
        Task<bool> DeleteFile(string? fileName, int place);

        string GetUniqueFileName(string? fileName);
        bool IsValidFile(string? fileName, string? placeHolder);
        bool IsValidFile(IFormFile? file);
        Task<(bool Success, string Result)> DownloadFile(int filePlace, string? fileName);
        Task<(bool Uploaded, string Result)> UploadMultipleFile(List<IFormFile>? files, int place);
    }
}
