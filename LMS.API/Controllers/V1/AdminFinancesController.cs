using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Wallet.DTOs;
using LMS.Application.Features.Wallet.Queries.GetAdminFinancialsOverview;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Admin · SubAdmin (per granted permission code)]
    /// Financial and operational KPIs overview for platform administrators (turnover, CCR commission, liability).
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Admin.Finances.Base)]
    [Tags("Admin: Finances & Overview")]
    [RoleAuthorize]
    public class AdminFinancesController : BaseController
    {
        /// <summary>
        /// Get high-level financial overview: gross turnover, platform commission collected, teacher balances, and liability.
        /// </summary>
        [HttpGet(ApiRoutes.Admin.Finances.Overview)]
        [RequirePermission(AdminPermission.FinancesRead)]
        [ProducesResponseType(typeof(ApiResponse<AdminFinancialOverviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetAdminFinancialsOverviewQuery(), cancellationToken);
            return FromResult(result);
        }
    }
}
