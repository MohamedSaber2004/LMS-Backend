using System.Net;
using System.Security.Claims;

namespace LMS.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid UserId
        {
            get
            {
                var userIdString = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User?.FindFirst("sub")?.Value;

                return Guid.TryParse(userIdString, out var userId)
                    ? userId
                    : Guid.Empty;
            }
        }

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public string? IpAddress
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return "127.0.0.1";

                var clientIp = CleanAndValidateIp(context.Request.Headers["X-Client-IP"].FirstOrDefault());
                if (!string.IsNullOrEmpty(clientIp)) return clientIp;

                var cfIp = CleanAndValidateIp(context.Request.Headers["CF-Connecting-IP"].FirstOrDefault());
                if (!string.IsNullOrEmpty(cfIp)) return cfIp;

                var realIp = CleanAndValidateIp(context.Request.Headers["X-Real-IP"].FirstOrDefault())
                             ?? CleanAndValidateIp(context.Request.Headers["X-Vercel-Forwarded-For"].FirstOrDefault())
                             ?? CleanAndValidateIp(context.Request.Headers["True-Client-IP"].FirstOrDefault());
                if (!string.IsNullOrEmpty(realIp)) return realIp;

                var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forwardedFor))
                {
                    var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var candidate in ips)
                    {
                        var cleaned = CleanAndValidateIp(candidate);
                        if (!string.IsNullOrEmpty(cleaned))
                        {
                            return cleaned;
                        }
                    }
                }

                var connectionIp = context.Connection.RemoteIpAddress;
                if (connectionIp != null)
                {
                    var cleaned = CleanAndValidateIp(connectionIp.ToString());
                    if (!string.IsNullOrEmpty(cleaned)) return cleaned;
                }

                return "127.0.0.1";
            }
        }

        private static string? CleanAndValidateIp(string? rawIp)
        {
            if (string.IsNullOrWhiteSpace(rawIp)) return null;

            var trimmed = rawIp.Trim();
            if (trimmed.Equals("unknown", StringComparison.OrdinalIgnoreCase)) return null;

            if (trimmed.StartsWith("[") && trimmed.Contains("]"))
            {
                var closingBracket = trimmed.IndexOf(']');
                trimmed = trimmed.Substring(1, closingBracket - 1);
            }
            else if (trimmed.Contains(':') && !trimmed.Contains("::") && trimmed.IndexOf(':') == trimmed.LastIndexOf(':'))
            {
                trimmed = trimmed.Split(':')[0];
            }

            if (IPAddress.TryParse(trimmed, out var ip))
            {
                if (ip.IsIPv4MappedToIPv6)
                {
                    ip = ip.MapToIPv4();
                }

                if (IPAddress.IsLoopback(ip) || ip.ToString() == "::1")
                {
                    return "127.0.0.1";
                }

                return ip.ToString();
            }

            return null;
        }

        public string CorrelationId => _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value
            ?? User?.FindFirst("email")?.Value;

        public string? FullName => User?.FindFirst("FullName")?.Value
            ?? User?.FindFirst(ClaimTypes.Name)?.Value;

        public string? Device
        {
            get
            {
                var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
                return ParseUserAgent(userAgent);
            }
        }

        public int? UserTypes
        {
            get
            {
                var userTypesClaim = User?.FindFirst("UserTypes")?.Value;
                return int.TryParse(userTypesClaim, out var userTypes)
                    ? userTypes
                    : null;
            }
        }

        private static string ParseUserAgent(string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown Device";

            var browser = "Browser";
            if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
                browser = "Edge";
            else if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase) && !userAgent.Contains("Edg", StringComparison.OrdinalIgnoreCase))
                browser = "Chrome";
            else if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase) && !userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
                browser = "Safari";
            else if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                browser = "Firefox";

            var os = string.Empty;
            if (userAgent.Contains("Windows NT 10.0", StringComparison.OrdinalIgnoreCase))
                os = "Windows 11/10";
            else if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                os = "Windows";
            else if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
                os = "iPhone";
            else if (userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
                os = "iPad";
            else if (userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("Mac OS", StringComparison.OrdinalIgnoreCase))
                os = "macOS";
            else if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
                os = "Android";
            else if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
                os = "Linux";

            return string.IsNullOrEmpty(os) ? browser : $"{browser} on {os}";
        }
    }
}
