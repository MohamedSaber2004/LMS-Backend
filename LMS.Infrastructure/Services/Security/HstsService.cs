using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Infrastructure.Services.Security
{
    public class HstsService : IHstsService
    {
        public void ConfigureHsts(IServiceCollection services, HstsOptions options)
        {
            services.AddHsts(hstsOptions =>
            {
                hstsOptions.Preload = options.Preload;
                hstsOptions.IncludeSubDomains = options.IncludeSubDomains;
                hstsOptions.MaxAge = TimeSpan.FromDays(options.MaxAgeDays);
            });
        }

        public void UseHsts(IApplicationBuilder app, HstsOptions options)
        {
            if (options.Enabled)
            {
                app.UseHsts();
            }
        }
    }
}
