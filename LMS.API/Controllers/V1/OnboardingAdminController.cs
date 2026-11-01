using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingOption;
using LMS.Application.Features.Onboarding.Commands.Admin.AddOnboardingQuestion;
using LMS.Application.Features.Onboarding.Commands.Admin.DeleteOnboardingOption;
using LMS.Application.Features.Onboarding.Commands.Admin.DeleteOnboardingQuestion;
using LMS.Application.Features.Onboarding.Commands.Admin.UpdateOnboardingOption;
using LMS.Application.Features.Onboarding.Commands.Admin.UpdateOnboardingQuestion;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Features.Onboarding.Queries.GetOnboardingQuestions;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Onboarding.Base)]
    [Authorize]
    [RoleAuthorize(UserType.Admin)]
    public class OnboardingAdminController : BaseController
    {
        /// <summary>
        /// Get all onboarding questions, including inactive ones.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Onboarding.AdminQuestions)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OnboardingQuestionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetQuestions(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetOnboardingQuestionsQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Create an onboarding question with its options.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Onboarding.AdminQuestions)]
        [ProducesResponseType(typeof(ApiResponse<OnboardingQuestionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddQuestion([FromBody] AddOnboardingQuestionCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update an onboarding question.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Onboarding.AdminQuestionById)]
        [ProducesResponseType(typeof(ApiResponse<OnboardingQuestionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateQuestion(
            [FromRoute] Guid id,
            [FromBody] UpdateOnboardingQuestionCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Delete an onboarding question.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Onboarding.AdminQuestionById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteQuestion([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteOnboardingQuestionCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Add an option to an existing question.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Onboarding.AdminQuestionOptions)]
        [ProducesResponseType(typeof(ApiResponse<OnboardingOptionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddOption(
            [FromRoute] Guid id,
            [FromBody] AddOnboardingOptionCommand command,
            CancellationToken cancellationToken)
        {
            command.QuestionId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update an option.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Onboarding.AdminOptionById)]
        [ProducesResponseType(typeof(ApiResponse<OnboardingOptionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOption(
            [FromRoute] Guid id,
            [FromBody] UpdateOnboardingOptionCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Delete an option.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Onboarding.AdminOptionById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOption([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteOnboardingOptionCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
