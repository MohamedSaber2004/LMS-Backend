using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Infrastructure.Services.Security
{
    public class AntiforgeryService : IAntiforgeryService
    {
        public void ConfigureAntiforgery(IServiceCollection services, AntiforgeryOptions options)
        {
            services.AddAntiforgery(antiforgeryOptions =>
            {
                antiforgeryOptions.HeaderName = options.HeaderName;
                antiforgeryOptions.Cookie.Name = options.CookieName;
                antiforgeryOptions.Cookie.HttpOnly = options.CookieHttpOnly;
                antiforgeryOptions.Cookie.SameSite = ParseSameSite(options.CookieSameSite);
            });
        }

        private static SameSiteMode ParseSameSite(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "lax" => SameSiteMode.Lax,
                "strict" => SameSiteMode.Strict,
                "none" => SameSiteMode.None,
                _ => SameSiteMode.Strict
            };
        }
    }
}
