using LMS.Application.Common.Options;
using Microsoft.Extensions.Configuration;

namespace LMS.Application.Common.services
{
    public class UploadPaths
    {
        private static UploadPathsOptions? Options;

        public static void Configure(IConfiguration configuration)
        {
            Options = configuration.GetSection("UploadPaths").Get<UploadPathsOptions>();
        }

        public static string Images => Options?.Images ?? string.Empty;
        public static string Videos => Options?.Videos  ?? string.Empty;
        public static string Audios => Options?.Audios ?? string.Empty;
        public static string Documents => Options?.Documents ?? string.Empty;

        public static string? GetPath(int place)
        {
            return place switch
            {
                0 => Images,
                1 => Videos,
                2 => Audios,
                3 => Documents,
                _ => string.Empty
            };
        }

        public static IEnumerable<string> GetAllPaths()
        {
            if (Options is null) yield break;
            yield return Options.Images;
            yield return Options.Videos;
            yield return Options.Audios;
            yield return Options.Documents;
        }
    }
}
