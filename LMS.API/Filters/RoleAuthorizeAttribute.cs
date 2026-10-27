using LMS.Application.Common.Models;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace LMS.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter, IAuthorizeData
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute()
        {
            _roles = Array.Empty<string>();
        }

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles ?? Array.Empty<string>();
        }

        public RoleAuthorizeAttribute(params UserType[] roles)
        {
            _roles = roles.Select(r => r.ToString()).ToArray();
        }

        public string? Policy { get; set; }

        public string? Roles { get; set; }

        public string? AuthenticationSchemes { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
            if (allowAnonymous || _roles.Length == 0) return;

            var user = context.HttpContext.User;
            var userTypesClaim = user.FindFirst("UserTypes")?.Value;
            var hasRequiredRole = false;

            foreach (var roleName in _roles)
            {
                if (user.IsInRole(roleName))
                {
                    hasRequiredRole = true;
                    break;
                }

                if (int.TryParse(userTypesClaim, out var userTypesInt) &&
                    Enum.TryParse<UserType>(roleName, ignoreCase: true, out var requiredType) &&
                    Enum.IsDefined(typeof(UserType), requiredType))
                {
                    if ((int)requiredType == 0 && userTypesInt == 0)
                    {
                        hasRequiredRole = true;
                        break;
                    }
                    else if ((int)requiredType != 0 && (userTypesInt & (int)requiredType) != 0)
                    {
                        hasRequiredRole = true;
                        break;
                    }
                    else if (userTypesInt == (int)requiredType)
                    {
                        hasRequiredRole = true;
                        break;
                    }
                }
            }

            if (!hasRequiredRole)
            {
                var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();
                var response = ApiResponse<object?>.Error(
                    new Dictionary<string, string[]>(),
                    localizer[LocalizationKeys.ExceptionMessages.Forbidden].Value,
                    StatusCodes.Status403Forbidden);

                context.Result = new ObjectResult(response)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
        }
    }
}
