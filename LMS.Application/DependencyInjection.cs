using LMS.Application.Common.Behaviours;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Application.Common.services;
using LMS.Application.Localization;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services,IConfiguration configuration)
        {
            services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));
            services.Configure<SecurityHeadersOptions>(configuration.GetSection("Security:Headers"));
            services.Configure<CorsOptions>(configuration.GetSection("Security:Cors"));
            services.Configure<AntiforgeryOptions>(configuration.GetSection("Security:Antiforgery"));
            services.Configure<RequestLimitsOptions>(configuration.GetSection("Security:RequestLimits"));
            services.Configure<HstsOptions>(configuration.GetSection("Security:Hsts"));
            services.Configure<IpRateLimitingOptions>(configuration.GetSection(IpRateLimitingOptions.SectionName));
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            services.Configure<IdentityOptions>(configuration.GetSection(nameof(IdentityOptions)));
            services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
            services.Configure<GoogleAuthSettings>(configuration.GetSection(nameof(GoogleAuthSettings)));

            services.AddMediatR(typeof(DependencyInjection).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            services.AddSingleton<ILocalizationProvider, JsonLocalizationProvider>();

            UploadPaths.Configure(configuration);

            return services;
        }
    }
}
