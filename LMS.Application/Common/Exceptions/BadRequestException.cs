using LMS.Application.Common.Exceptions.Base;
using LMS.Application.Localization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Common.Exceptions
{
    public class BadRequestException : BaseCustomException
    {
        public IDictionary<string, string[]> Errors { get; }

        public BadRequestException()
            : base(LocalizationKeys.ExceptionMessages.BadRequest, StatusCodes.Status400BadRequest)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string localizationKey, params object[] args)
            : base(localizationKey, StatusCodes.Status400BadRequest, args)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, new[] { localizationKey } } };
        }

        public BadRequestException(string[] errors, string localizationKey = LocalizationKeys.ExceptionMessages.BadRequest)
            : base(
                errors != null && errors.Length > 0
                    ? $"{localizationKey} ({string.Join("; ", errors)})"
                    : localizationKey,
                localizationKey,
                StatusCodes.Status400BadRequest)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, errors ?? Array.Empty<string>() } };
        }

        public BadRequestException(IDictionary<string, string[]> errors, string localizationKey = LocalizationKeys.ExceptionMessages.BadRequest)
            : base(
                errors != null && errors.Any()
                    ? $"{localizationKey} ({string.Join("; ", errors.Select(kv => $"{kv.Key}: [{string.Join(", ", kv.Value)}]"))})"
                    : localizationKey,
                localizationKey,
                StatusCodes.Status400BadRequest)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        public BadRequestException(string localizationKey, Exception innerException, params object[] args)
            : base(localizationKey, innerException, StatusCodes.Status400BadRequest, args)
        {
            Errors = new Dictionary<string, string[]> { { string.Empty, new[] { localizationKey } } };
        }
    }
}
