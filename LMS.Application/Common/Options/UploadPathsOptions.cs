using LMS.Domain.Enums;

namespace LMS.Application.Common.Options
{
    public class UploadPathsOptions
    {
        public const string SectionName = "UploadPaths";

        public string Images { get; set; } = null!;
        public string Videos { get; set; } = null!;
        public string Audios { get; set; } = null!;
        public string Documents { get; set; } = null!;
    }
}
