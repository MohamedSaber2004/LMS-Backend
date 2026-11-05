using FluentValidation.Validators;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace LMS.Application.Features.Courses
{
    internal static class CoursesMediaChecks
    {
        internal const int MaxUrlLength = 500;
        internal const long MaxVideoBytes = 1024L * 1024 * 1024;
        internal const long MaxStudyBookBytes = 50L * 1024 * 1024;
        internal const long MaxCoverBytes = 5L * 1024 * 1024;

        internal static void ValidateVideo(
            IFormFile? file,
            string fieldName,
            CustomContext context,
            IVideoValidator validator,
            IStringLocalizer<Messages> localizer)
        {
            Check(
                file,
                fieldName,
                context,
                validator.IsValidVideo,
                MaxVideoBytes,
                (int)(MaxVideoBytes / (1024 * 1024)),
                LocalizationKeys.Courses.InvalidVideoFile,
                localizer);
        }

        internal static void ValidateStudyBook(
            IFormFile? file,
            string fieldName,
            CustomContext context,
            IFileValidator validator,
            IStringLocalizer<Messages> localizer)
        {
            Check(
                file,
                fieldName,
                context,
                validator.IsValidFile,
                MaxStudyBookBytes,
                (int)(MaxStudyBookBytes / (1024 * 1024)),
                LocalizationKeys.Courses.InvalidStudyBookFile,
                localizer);
        }

        internal static void ValidateCover(
            IFormFile? file,
            string fieldName,
            CustomContext context,
            IImageValidator validator,
            IStringLocalizer<Messages> localizer)
        {
            Check(
                file,
                fieldName,
                context,
                validator.IsValidImage,
                MaxCoverBytes,
                (int)(MaxCoverBytes / (1024 * 1024)),
                LocalizationKeys.Courses.InvalidCoverFile,
                localizer);
        }

        private static void Check(
            IFormFile? file,
            string fieldName,
            CustomContext context,
            Func<IFormFile?, bool> isValidFormat,
            long maxBytes,
            int maxMegabytes,
            string invalidFormatKey,
            IStringLocalizer<Messages> localizer)
        {
            if (file is null || file.Length == 0)
            {
                return;
            }

            if (file.Length > maxBytes)
            {
                context.AddFailure(
                    fieldName,
                    localizer[LocalizationKeys.Courses.FileTooLarge, maxMegabytes].Value);
                return;
            }

            if (!isValidFormat(file))
            {
                context.AddFailure(fieldName, localizer[invalidFormatKey].Value);
            }
        }
    }
}
