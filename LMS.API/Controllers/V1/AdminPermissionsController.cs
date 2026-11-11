using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.AdminPermissions.Commands.SetUserPermissions;
using LMS.Application.Features.AdminPermissions.DTOs;
using LMS.Application.Features.AdminPermissions.Queries.GetPermissionCatalog;
using LMS.Application.Features.AdminPermissions.Queries.GetUserPermissions;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Admin (all permissions) · SubAdmin (per granted permission code)]
    /// Admin board access control: the permission catalog and the permission codes granted to a user account.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Admin.Permissions.Base)]
    [Tags("Admin: Permissions")]
    [RoleAuthorize]
    public class AdminPermissionsController : BaseController
    {
        /// <summary>
        /// List every permission code grouped by feature, with the calling account's effective permissions.
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Permissions.Catalog)]
        [RequirePermission(AdminPermission.PermissionsRead)]
        [ProducesResponseType(typeof(ApiResponse<PermissionCatalogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetPermissionCatalogQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// List the permission codes granted to a user account.
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Permissions.ForUser)]
        [RequirePermission(AdminPermission.PermissionsRead)]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetForUser([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetUserPermissionsQuery { UserId = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Replace the permission codes granted to a user account. Sending an empty list revokes every grant.
        /// </summary>
        [HttpPut(ApiRoutes.Admin.Permissions.ForUser)]
        [RequirePermission(AdminPermission.PermissionsUpdate)]
        [ProducesResponseType(typeof(ApiResponse<UserPermissionsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetForUser(
            [FromRoute] Guid id,
            [FromBody] SetUserPermissionsCommand command,
            CancellationToken cancellationToken)
        {
            command.UserId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
