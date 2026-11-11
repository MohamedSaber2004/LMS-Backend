using System.Security.Claims;
using LMS.Application.Common.Models;
using LMS.Application.Common.Security;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace LMS.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly AdminPermission _permission;

        public RequirePermissionAttribute(AdminPermission permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var endpointMetadata = context.ActionDescriptor.EndpointMetadata;
            if (endpointMetadata.OfType<IAllowAnonymous>().Any()) return;

            var user = context.HttpContext.User;
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();

            if (user.Identity?.IsAuthenticated != true)
            {
                Deny(context, localizer, StatusCodes.Status401Unauthorized, LocalizationKeys.ExceptionMessages.Unauthorized);
                return;
            }

            if (IsSuperAdmin(user)) return;

            var grantedCodes = user
                .FindAll(PermissionCatalog.ClaimType)
                .Select(claim => claim.Value)
                .Where(value => int.TryParse(value, out _))
                .ToHashSet(StringComparer.Ordinal);

            if (grantedCodes.Contains(((int)_permission).ToString())) return;

            var feature = localizer[PermissionCatalog.FeatureDisplayNameKey(_permission)].Value;
            var action = localizer[PermissionCatalog.ActionDisplayNameKey(_permission)].Value;

            Deny(
                context,
                localizer,
                StatusCodes.Status403Forbidden,
                LocalizationKeys.Permissions.ForbiddenMissingPermission,
                $"{feature} ({action})");
        }

        private static bool IsSuperAdmin(ClaimsPrincipal user)
        {
            if (user.IsInRole(nameof(UserType.Admin)))
            {
                return true;
            }

            return int.TryParse(user.FindFirst("UserTypes")?.Value, out var userTypes)
                   && (userTypes & (int)UserType.Admin) != 0;
        }

        private static void Deny(
            AuthorizationFilterContext context,
            IStringLocalizer<Messages> localizer,
            int statusCode,
            string localizationKey,
            params object[] args)
        {
            var message = args.Length > 0
                ? localizer[localizationKey, args].Value
                : localizer[localizationKey].Value;

            var response = ApiResponse<object?>.Error(new Dictionary<string, string[]>(), message, statusCode);

            context.Result = new ObjectResult(response) { StatusCode = statusCode };
        }
    }
}
