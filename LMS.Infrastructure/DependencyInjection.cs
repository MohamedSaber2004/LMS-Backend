using LMS.Application.Common.Interfaces;
using LMS.Infrastructure.Services.Security;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ISecurityHeadersService, SecurityHeadersService>();

            return services;
        }
    }
}
