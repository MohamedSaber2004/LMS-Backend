using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpArticle;
using LMS.Application.Features.HelpCenter.Commands.Admin.AddHelpCategory;
using LMS.Application.Features.HelpCenter.Commands.Admin.DeleteHelpArticle;
using LMS.Application.Features.HelpCenter.Commands.Admin.DeleteHelpCategory;
using LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpArticle;
using LMS.Application.Features.HelpCenter.Commands.Admin.UpdateHelpCategory;
using LMS.Application.Features.HelpCenter.DTOs;
using LMS.Application.Features.HelpCenter.Queries.GetHelpArticlesPagginated;
using LMS.Application.Features.HelpCenter.Queries.GetHelpCategories;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.HelpCenter.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Admin)]
    public class HelpCenterAdminController : BaseController
    {
        /// <summary>
        /// Get every help category, inactive ones included, with their active article counts.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.HelpCenter.AdminCategories)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<HelpCategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetHelpCategoriesQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Create a help category. Names are unique once trimmed.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.HelpCenter.AdminCategories)]
        [ProducesResponseType(typeof(ApiResponse<HelpCategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddCategory(
            [FromBody] AddHelpCategoryCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update a help category's name, order and visibility.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.HelpCenter.AdminCategoryById)]
        [ProducesResponseType(typeof(ApiResponse<HelpCategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(
            [FromRoute] Guid id,
            [FromBody] UpdateHelpCategoryCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Soft delete a help category. A category that still holds articles cannot be deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.HelpCenter.AdminCategoryById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteHelpCategoryCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get help articles with pagination, unpublished rows included. Filters by category,
        /// keyword and visibility.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.HelpCenter.AdminArticles)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<HelpArticleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetArticlesPagginated(
            [FromQuery] GetHelpArticlesPagginatedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Create a help article inside an existing category. The answer is stored as plain text.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.HelpCenter.AdminArticles)]
        [ProducesResponseType(typeof(ApiResponse<HelpArticleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddArticle(
            [FromBody] AddHelpArticleCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update a help article's question, answer, order and visibility. Moving an article to
        /// another category is not supported; delete and recreate it instead.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.HelpCenter.AdminArticleById)]
        [ProducesResponseType(typeof(ApiResponse<HelpArticleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(
            [FromRoute] Guid id,
            [FromBody] UpdateHelpArticleCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Soft delete a help article.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.HelpCenter.AdminArticleById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteArticle([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteHelpArticleCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
