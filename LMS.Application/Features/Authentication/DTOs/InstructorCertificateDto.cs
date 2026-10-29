using LMS.Domain.Enums;

namespace LMS.Application.Features.Authentication.DTOs
{
    public class InstructorCertificateDto
    {
        public InstructorCertificateDto() { }

        public InstructorCertificateDto(
            Guid id,
            string filePath,
            string originalFileName,
            InstructorCertificateType type)
        {
            Id = id;
            FilePath = filePath;
            OriginalFileName = originalFileName;
            Type = type;
        }

        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public InstructorCertificateType Type { get; set; }
    }
}
