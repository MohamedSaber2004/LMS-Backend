using Asp.Versioning;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Features.HelpCenter.Queries.GetActiveHelpArticleById;
using LMS.Application.Features.HelpCenter.Queries.GetActiveHelpArticlesPagginated;
using LMS.Application.Features.HelpCenter.Queries.GetActiveHelpCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Public]
    /// Read-only browsing of active help center categories and articles.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.HelpCenter.Base)]
    [Tags("Help Center (Public)")]
    public class HelpCenterController : BaseController
    {
        /// <summary>
        /// Get the published help categories with their article counts.
        /// Only active categories are returned; no sign-in is required.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.HelpCenter.Categories)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelpCategoryDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetActiveHelpCategoriesQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get published help articles with pagination, optionally filtered by category or a
        /// keyword matched against the question and the answer. Answers are plain text with the
        /// author's line breaks preserved.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.HelpCenter.Articles)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<HelpArticleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetArticlesPagginated(
            [FromQuery] GetActiveHelpArticlesPagginatedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get one published help article. Unpublished content answers 404.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.HelpCenter.ArticleById)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<HelpArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetArticleById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetActiveHelpArticleByIdQuery { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
