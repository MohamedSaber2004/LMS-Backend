using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.Commands.PurchasePackage;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Features.Packages.Queries.GetMyPackages;
using LMS.Application.Features.Packages.Queries.GetPackageCatalog;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Learner (Catalog is Public)]
    /// Student API for browsing the admin-defined package tiers, purchasing hour packages and tracking remaining balances.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Learner.Packages.Base)]
    [Tags("Learner: Packages")]
    public class LearnerPackagesController : BaseController
    {
        /// <summary>
        /// Get the package tiers configured by the admin, priced for one specific teacher (available for the landing page and mobile).
        /// </summary>
        [HttpGet(ApiRoutes.Learner.Packages.Catalog)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PackageCatalogItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCatalog([FromQuery] Guid instructorId, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetPackageCatalogQuery { InstructorId = instructorId }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Instantly activate a package of hours for a teacher. The hours must match a configured tier. Requires learner login.
        /// </summary>
        [HttpPost(ApiRoutes.Learner.Packages.Purchase)]
        [Authorize]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<LearnerPackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Purchase([FromBody] PurchasePackageCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// View learner's active package balances and remaining session credits. Requires learner login.
        /// </summary>
        [HttpGet(ApiRoutes.Learner.Packages.MyBalances)]
        [Authorize]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<LearnerPackageDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBalances([FromQuery] Guid? instructorId, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetMyPackagesQuery { InstructorId = instructorId }, cancellationToken);
            return FromResult(result);
        }
    }
}

