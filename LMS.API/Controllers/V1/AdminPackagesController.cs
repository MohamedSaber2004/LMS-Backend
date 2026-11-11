using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.Commands.AdjustPackageHours;
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
    /// Admin management of the purchasable package tiers and of student hour packages across the system, including manual hour adjustments.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Admin.Packages.Base)]
    [Tags("Admin: Packages")]
    [RoleAuthorize]
    public class AdminPackagesController : BaseController
    {
        /// <summary>
        /// List all student purchased packages across the platform.
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
        /// Adjust remaining hours of a learner's package (for administrative corrections or compensation).
        /// </summary>
        [HttpPost(ApiRoutes.Admin.Packages.AdjustHours)]
        [RequirePermission(AdminPermission.PackagesUpdate)]
        [ProducesResponseType(typeof(ApiResponse<LearnerPackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdjustHours(
            [FromRoute] Guid id,
            [FromBody] AdjustPackageHoursCommand command,
            CancellationToken cancellationToken)
        {
            command.PackageId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// List the package tiers that learners can buy.
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
        /// Add a package tier to the catalog.
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
        /// Update a package tier. Existing learner packages keep the name and hours they were bought with.
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
        /// Remove a package tier from the catalog.
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
