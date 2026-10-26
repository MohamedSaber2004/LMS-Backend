using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Http;

namespace LMS.Infrastructure.Services.Security
{
    public class SecurityHeadersService : ISecurityHeadersService
    {
        public void AddSecurityHeaders(HttpContext context, SecurityHeadersOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.XFrameOptions))
            {
                context.Response.Headers["X-Frame-Options"] = options.XFrameOptions;
            }
            context.Response.Headers["X-Content-Type-Options"] = options.XContentTypeOptions;
            context.Response.Headers["X-XSS-Protection"] = options.XssProtection;
            context.Response.Headers["Content-Security-Policy"] = options.ContentSecurityPolicy;
            context.Response.Headers["Referrer-Policy"] = options.ReferrerPolicy;
            context.Response.Headers["Permissions-Policy"] = options.PermissionsPolicy;

            if (context.Request.IsHttps)
            {
                context.Response.Headers["Strict-Transport-Security"] = options.StrictTransportSecurity;
            }
        }
    }
}
