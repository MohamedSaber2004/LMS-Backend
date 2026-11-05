using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.Commands.EnrollCourse;
using LMS.Application.Features.Courses.Commands.SubmitQuiz;
using LMS.Application.Features.Courses.Commands.UpdateLessonProgress;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Features.Courses.Queries.GetActiveCourseById;
using LMS.Application.Features.Courses.Queries.GetActiveCoursesPagginated;
using LMS.Application.Features.Courses.Queries.GetLessonContent;
using LMS.Application.Features.Courses.Queries.GetMyCoursesPagginated;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Courses.Base)]
    public class CoursesController : BaseController
    {
        /// <summary>
        /// Get the published course library with pagination, optionally filtered by a keyword
        /// matched against title and description, or by level. Every card carries the count of
        /// published lessons; enrollment state is not included here.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Courses.Pagginated)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<CourseListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCoursesPagginated(
            [FromQuery] GetActiveCoursesPagginatedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get one published course with its lesson list in order. Lessons hide the video link,
        /// only telling whether a study book exists. Guests get IsEnrolled false.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Courses.CourseById)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<CourseDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetActiveCourseByIdQuery { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get one lesson's video link, study book link, quiz questions and the signed-in
        /// learner's saved position plus last quiz result. Free demo lessons answer for guests;
        /// every other lesson answers 403 until the learner enrolls in its course.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Courses.LessonContent)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LessonContentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLessonContent(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetLessonContentQuery { LessonId = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get the courses the signed-in learner is enrolled in, most recent first, with
        /// completed lesson counts and the last lesson opened.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Courses.MyCourses)]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<MyCourseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyCoursesPagginated(
            [FromQuery] GetMyCoursesPagginatedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Enroll the signed-in learner in a published course. Enrolling is free and idempotent;
        /// enrolling twice returns the same lesson list.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Courses.Enroll)]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<CourseDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Enroll(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new EnrollCourseCommand { CourseId = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Save the learner's watched position in a lesson, and mark it completed once the video
        /// ends. Creates the progress row on the first call. Locked lessons answer 403.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Courses.LessonProgress)]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<LessonProgressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLessonProgress(
            [FromRoute] Guid id,
            [FromBody] UpdateLessonProgressCommand command,
            CancellationToken cancellationToken)
        {
            command.LessonId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Submit the learner's quiz answers for a lesson and get the score back with the answers
        /// given. Any score keeps the next lesson open. Locked lessons answer 403.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Courses.LessonQuiz)]
        [RoleAuthorize(UserType.Learner)]
        [ProducesResponseType(typeof(ApiResponse<QuizResultDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitQuiz(
            [FromRoute] Guid id,
            [FromBody] SubmitQuizCommand command,
            CancellationToken cancellationToken)
        {
            command.LessonId = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }
    }
}
