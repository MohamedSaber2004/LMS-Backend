using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Onboarding.Commands.SubmitOnboardingAnswers;
using LMS.Application.Features.Onboarding.DTOs;
using LMS.Application.Features.Onboarding.Queries.GetActiveOnboardingQuestions;
using LMS.Application.Features.Onboarding.Queries.GetInstructorsForLearner;
using LMS.Application.Features.Onboarding.Queries.GetMyOnboardingAnswers;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Learner]
    /// Learner onboarding wizard: answer the active question set and browse instructors matched to those answers.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Onboarding.Base)]
    [Tags("Learner: Onboarding")]
    [Authorize]
    [RoleAuthorize(UserType.Learner)]
    public class OnboardingController : BaseController
    {
        /// <summary>
        /// Get the active onboarding questions shown to learners.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Onboarding.Questions)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OnboardingQuestionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetQuestions(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetActiveOnboardingQuestionsQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Submit (or update) the learner's onboarding answers.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Onboarding.Answers)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SubmitAnswers([FromBody] SubmitOnboardingAnswersCommand command, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get the learner's saved onboarding answers, including the expected lesson budget.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Onboarding.MyAnswers)]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<OnboardingAnswerGroupDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyAnswers(CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetMyOnboardingAnswersQuery(), cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// List approved instructors, optionally filtered by the learner's budget.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Onboarding.Instructors)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<InstructorCardDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetInstructors([FromQuery] GetInstructorsForLearnerQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }
    }
}
