using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Common.Interfaces
{
    public interface ISecurityHeadersService
    {
        void AddSecurityHeaders(HttpContext context, SecurityHeadersOptions options);
    }
}
