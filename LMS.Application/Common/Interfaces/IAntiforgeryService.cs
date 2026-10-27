using LMS.Application.Common.Options;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Application.Common.Interfaces
{
    public interface IAntiforgeryService
    {
        void ConfigureAntiforgery(IServiceCollection services, AntiforgeryOptions options);
    }
}
