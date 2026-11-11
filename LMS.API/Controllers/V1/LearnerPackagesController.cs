using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Packages.Commands.PurchasePackage;
using LMS.Application.Features.Packages.DTOs;
using LMS.Application.Features.Packages.Queries.GetMyPackages;
using LMS.Application.Features.Packages.Queries.GetPackageCatalog;
using LMS.Application.Features.Packages.Queries.GetLearnerWallet;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Learner / Public Guest]
    /// Student API for browsing available subscription plans, subscribing to a plan, and viewing the virtual sessions wallet balance.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Learner.Packages.Base)]
    [Tags("Learner: Packages")]
    public class LearnerPackagesController : BaseController
    {
        /// <summary>
        /// [Public / Student] Browse available subscription plan tiers (e.g. 4, 8, 12, 16 sessions) configured by admin.
        /// </summary>
        [HttpGet(ApiRoutes.Learner.Packages.AvailablePlans)]
        [HttpGet(ApiRoutes.Learner.Packages.Catalog)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<PackageCatalogItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetPackageCatalogQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Student] Subscribe to a subscription plan (activates the fixed session quota in the student's wallet without upfront charge).
        /// </summary>
        [HttpPost(ApiRoutes.Learner.Packages.Subscribe)]
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
        /// [Student] View student's virtual sessions wallet: total plan sessions, remaining sessions, full (50m) and half (25m) session counts, and free trial status.
        /// </summary>
        [HttpGet(ApiRoutes.Learner.Packages.Wallet)]
        [Authorize]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<LearnerSessionsWalletDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWallet(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetLearnerWalletQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// [Student] View student's detailed package subscription records and history. Requires student login.
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

