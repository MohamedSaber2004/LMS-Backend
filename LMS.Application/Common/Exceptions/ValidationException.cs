using FluentValidation.Results;
using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Exceptions
{
    public class ValidationException : BaseCustomException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException()
            : base(LocalizationKeys.ExceptionMessages.Validation, StatusCodes.Status400BadRequest)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status400BadRequest, args)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, new[] { localizationKey } } };
        }

        public ValidationException(IEnumerable<ValidationFailure> failures, string localizationKey = LocalizationKeys.ExceptionMessages.Validation)
            : base(
                BuildErrorMessage(failures, localizationKey),
                localizationKey,
                StatusCodes.Status400BadRequest)
        {
            Errors = failures?
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray())
                ?? new Dictionary<string, string[]>();
        }

        public ValidationException(IDictionary<string, string[]> errors, string localizationKey = LocalizationKeys.ExceptionMessages.Validation)
            : base(
                BuildErrorMessage(errors, localizationKey),
                localizationKey,
                StatusCodes.Status400BadRequest)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        private static string BuildErrorMessage(IEnumerable<ValidationFailure>? failures, string localizationKey)
        {
            if (failures == null || !failures.Any())
                return localizationKey;

            var details = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
            return $"{localizationKey} ({details})";
        }

        private static string BuildErrorMessage(IDictionary<string, string[]>? errors, string localizationKey)
        {
            if (errors == null || !errors.Any())
                return localizationKey;

            var details = string.Join("; ", errors.Select(kv => $"{kv.Key}: [{string.Join(", ", kv.Value)}]"));
            return $"{localizationKey} ({details})";
        }
    }
}
