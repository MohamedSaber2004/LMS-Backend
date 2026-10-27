using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Interfaces
{
    public interface IImageValidator
    {
        Task<(bool Uploaded, string Result)> UploadImage(IFormFile? file, int place);
        Task<(bool Uploaded, string Result)> UploadMultipleImage(List<IFormFile>? files, int place);
        bool ImageIsExisted(string? fullImagePath);
        Task<bool> DeleteImage(string? fileName, int place);
        string GetUniqueFileName(string? fileName);

        bool IsValidImage(IFormFile? file);
        bool IsValidImage(string? imageName, string? placeHolder);

        Task<IFormFile?> ConvertImageToFormFile(string? imageUrl, CancellationToken cancellationToken = default);
    }
}
