using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.Commands.CreatePackageTier;
using LMS.Application.Features.Packages.Commands.DeletePackageTier;
using LMS.Application.Features.Packages.Commands.UpdatePackageTier;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Features.Packages.Queries.GetAdminPackages;
using LMS.Application.Features.Packages.Queries.GetPackageTiers;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Admin · SubAdmin (per granted permission code)]
    /// Admin management of the platform's subscription plan tiers catalog (e.g. 4, 8, 12 sessions) and tracking of all students' active/past subscriptions.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Admin.Packages.Base)]
    [Tags("Admin: Packages")]
    [RoleAuthorize]
    public class AdminPackagesController : BaseController
    {
        /// <summary>
        /// [Admin] List all students' active and past subscriptions across the platform (shows student name, plan name, remaining sessions balance, and status).
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Packages.GetAll)]
        [RequirePermission(AdminPermission.PackagesRead)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<AdminPackageItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetAdminPackagesQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Admin] List all subscription plan tiers in the catalog (the available plans that students can subscribe to).
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Packages.Tiers)]
        [RequirePermission(AdminPermission.PackageTiersRead)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PackageTierDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTiers(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetPackageTiersQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Admin] Add a new subscription plan tier to the platform catalog (e.g. 6 sessions, 12 sessions).
        /// </summary>
        [HttpPost(ApiRoutes.Admin.Packages.Tiers)]
        [RequirePermission(AdminPermission.PackageTiersAdd)]
        [ProducesResponseType(typeof(ApiResponse<PackageTierDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTier(
            [FromBody] CreatePackageTierCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Admin] Update an existing subscription plan tier. Existing student subscriptions keep the name and sessions quota they were enrolled with.
        /// </summary>
        [HttpPut(ApiRoutes.Admin.Packages.TierById)]
        [RequirePermission(AdminPermission.PackageTiersUpdate)]
        [ProducesResponseType(typeof(ApiResponse<PackageTierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTier(
            [FromRoute] Guid id,
            [FromBody] UpdatePackageTierCommand command,
            CancellationToken cancellationToken)
        {
            command.TierId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Admin] Remove a subscription plan tier from the platform catalog.
        /// </summary>
        [HttpDelete(ApiRoutes.Admin.Packages.TierById)]
        [RequirePermission(AdminPermission.PackageTiersDelete)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTier(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeletePackageTierCommand { TierId = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
