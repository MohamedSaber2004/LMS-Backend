using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Infrastructure.Services.Security
{
    public class CorsPolicyService : ICorsPolicyService
    {
        public void ConfigureCors(IServiceCollection services, CorsOptions options)
        {
            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy(options.PolicyName, policy =>
                {
                    policy.WithOrigins(options.AllowedOrigins.ToArray())
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
        }

        public void UseCors(IApplicationBuilder app, string policyName)
        {
            app.UseCors(policyName);
        }
    }
}
