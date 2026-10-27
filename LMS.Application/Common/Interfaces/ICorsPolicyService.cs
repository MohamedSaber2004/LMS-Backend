using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Application.Common.Interfaces
{
    public interface ICorsPolicyService
    {
        void ConfigureCors(IServiceCollection services, CorsOptions options);
        void UseCors(IApplicationBuilder app, string policyName);
    }
}
