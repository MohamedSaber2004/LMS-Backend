using LMS.Application.Common.Extensions;
using LMS.Application.Common.Interfaces;
using LMS.Domain.Enums;
using System.Globalization;

namespace LMS.API.Services
{
    public class CurrentLanguageService : ICurrentLanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CurrentLanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Language Language => AppLanguageExtensions.FromCode(ResolveCulture());

        public string LanguageCode => Language.ToCode();

        private string? ResolveCulture()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request is null)
                return CultureInfo.CurrentUICulture?.Name;

            var queryCulture = request.Query["Language"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(queryCulture))
                return queryCulture;

            var headers = request.Headers;

            var headerCulture = headers["Language"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(headerCulture))
                return headerCulture;

            return CultureInfo.CurrentUICulture?.Name;
        }
    }
}
