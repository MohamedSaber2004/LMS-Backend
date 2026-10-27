using LMS.Application.Common.Interfaces;
using LMS.Infrastructure.Services.Attachments;
using LMS.Infrastructure.Services.Security;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ISecurityHeadersService, SecurityHeadersService>();
            services.AddSingleton<ICorsPolicyService, CorsPolicyService>();
            services.AddSingleton<IAntiforgeryService, AntiforgeryService>();
            services.AddSingleton<IHstsService, HstsService>();

            services.AddHttpClient();

            services.AddScoped<IBaseFileService, BaseFileService>();
            services.AddScoped<IAudioValidator, AudioValidator>();
            services.AddScoped<IFileValidator, FileValidator>();
            services.AddScoped<IImageValidator, ImageValidator>();
            services.AddScoped<IVideoValidator, VideoValidator>();

            return services;
        }
    }
}
