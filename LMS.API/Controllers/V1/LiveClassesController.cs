using System;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.LiveClasses.Commands.EndLiveClass;
using LMS.Application.Features.LiveClasses.Commands.JoinLiveClass;
using LMS.Application.Features.LiveClasses.Commands.LeaveLiveClass;
using LMS.Application.Features.LiveClasses.Commands.RefreshToken;
using LMS.Application.Features.LiveClasses.Commands.StartLiveClass;
using LMS.Application.Features.LiveClasses.DTOs;
using LMS.Application.Features.LiveClasses.Queries.GetAgoraConfig;
using LMS.Application.Features.LiveClasses.Queries.GetLiveClassSession;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.LiveClasses.Base)]
    [Authorize]
    public class LiveClassesController : BaseController
    {
        /// <summary>
        /// Get Agora configuration parameters (AppId, enabled status, join lead time).
        /// </summary>
        [HttpGet(ApiRoutes.LiveClasses.Config)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<AgoraConfigDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConfig(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetAgoraConfigQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Start the live class room and acquire Agora publisher token (Instructor only).
        /// </summary>
        [HttpPost(ApiRoutes.LiveClasses.Start)]
        [RoleAuthorize(UserType.Instructor)]
        [ProducesResponseType(typeof(ApiResponse<LiveClassTokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Start(
            [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new StartLiveClassCommand(sessionId), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Join an ongoing or upcoming live class and acquire Agora token (Instructor or Learner).
        /// </summary>
        [HttpPost(ApiRoutes.LiveClasses.Join)]
        [RoleAuthorize(UserType.Instructor, UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<LiveClassTokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Join(
            [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new JoinLiveClassCommand(sessionId), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Leave a live class session and record exit timestamp.
        /// </summary>
        [HttpPost(ApiRoutes.LiveClasses.Leave)]
        [RoleAuthorize(UserType.Instructor, UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Leave(
            [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new LeaveLiveClassCommand(sessionId), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// End the live class room permanently and close all participant records (Instructor only).
        /// </summary>
        [HttpPost(ApiRoutes.LiveClasses.End)]
        [RoleAuthorize(UserType.Instructor)]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> End(
            [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new EndLiveClassCommand(sessionId), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get detailed live class session status, active participants, and join permissions.
        /// </summary>
        [HttpGet(ApiRoutes.LiveClasses.GetSession)]
        [RoleAuthorize(UserType.Instructor, UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<LiveClassDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSession(
            [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetLiveClassSessionQuery(sessionId), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Refresh the Agora RTC token during a live call before expiration.
        /// </summary>
        [HttpPost(ApiRoutes.LiveClasses.Token)]
        [RoleAuthorize(UserType.Instructor, UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RefreshToken(
            [FromRoute] Guid sessionId,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new RefreshTokenCommand(sessionId), cancellationToken);
            return FromResult(result);
        }
    }
}

