using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Application.Common.Interfaces
{
    public interface IHstsService
    {
        void ConfigureHsts(IServiceCollection services, HstsOptions options);
        void UseHsts(IApplicationBuilder app, HstsOptions options);
    }
}
