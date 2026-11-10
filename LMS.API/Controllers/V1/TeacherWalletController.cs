using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Wallet.Commands.RequestWalletWithdrawal;
using LMS.Application.Features.Wallet.DTOs;
using LMS.Application.Features.Wallet.Queries.GetTeacherWalletSummary;
using LMS.Application.Features.Wallet.Queries.GetTeacherWalletTransactions;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Instructor (Teacher)]
    /// Teacher financial wallet, available balance, transaction history, instant payouts, and withdrawal requests.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Wallet.Base)]
    [Tags("Teacher: Wallet")]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherWalletController : BaseController
    {
        /// <summary>
        /// Get teacher's wallet summary, available balance, total earnings, and current CCR.
        /// </summary>
        [HttpGet(ApiRoutes.Teacher.Wallet.Summary)]
        [ProducesResponseType(typeof(ApiResponse<TeacherWalletSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetTeacherWalletSummaryQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Request a payout withdrawal from available wallet balance.
        /// </summary>
        [HttpPost(ApiRoutes.Teacher.Wallet.Withdraw)]
        [ProducesResponseType(typeof(ApiResponse<TeacherWalletSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Withdraw(
            [FromBody] RequestWalletWithdrawalCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get paginated transaction history for the teacher's wallet.
        /// </summary>
        [HttpGet(ApiRoutes.Teacher.Wallet.Transactions)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<TeacherWalletTransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] GetTeacherWalletTransactionsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }
    }
}

