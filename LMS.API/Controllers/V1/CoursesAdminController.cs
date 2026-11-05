using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.Courses.Commands.Admin.AddCourse;
using LMS.Application.Features.Courses.Commands.Admin.AddLesson;
using LMS.Application.Features.Courses.Commands.Admin.AddQuizQuestion;
using LMS.Application.Features.Courses.Commands.Admin.DeleteCourse;
using LMS.Application.Features.Courses.Commands.Admin.DeleteLesson;
using LMS.Application.Features.Courses.Commands.Admin.DeleteQuizQuestion;
using LMS.Application.Features.Courses.Commands.Admin.UpdateCourse;
using LMS.Application.Features.Courses.Commands.Admin.UpdateLesson;
using LMS.Application.Features.Courses.Commands.Admin.UpdateQuizQuestion;
using LMS.Application.Features.Courses.DTOs;
using LMS.Application.Features.Courses.Queries.Admin.GetAdminCourseById;
using LMS.Application.Features.Courses.Queries.Admin.GetCoursesPagginated;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Courses.Base)]
    [RoleAuthorize(UserType.Admin)]
    public class CoursesAdminController : BaseController
    {
        /// <summary>
        /// Get every course, unpublished ones included, with their published lesson counts.
        /// Filters by keyword, level and visibility.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Courses.AdminCourses)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<CourseListItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCoursesPagginated(
            [FromQuery] GetCoursesPagginatedQuery query,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Get one course with its whole tree: lessons, their quiz questions and the correct
        /// answer of every question.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Courses.AdminCourseById)]
        [ProducesResponseType(typeof(ApiResponse<CourseAdminDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetAdminCourseByIdQuery { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Create a course. Levels are Beginner, Elementary, Intermediate and Advanced.
        /// The cover is optional: send coverFile (multipart) or coverUrl, a file always wins.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Courses.AdminCourses)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<CourseListItemDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddCourse(
            [FromForm] AddCourseCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update a course's title, description, level, order and visibility. Omitting both
        /// coverFile and coverUrl keeps the current cover; sending coverUrl empty removes it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Courses.AdminCourseById)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<CourseListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCourse(
            [FromRoute] Guid id,
            [FromForm] UpdateCourseCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Soft delete a course. A course that still holds lessons cannot be deleted; delete the
        /// lessons first.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Courses.AdminCourseById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCourse([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteCourseCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Create a lesson inside a course. Send the video either as videoFile (multipart) or as
        /// videoUrl; a file always wins over the link. studyBookFile is the optional summary
        /// document. Marking isFreeDemo keeps the lesson open to guests. The response carries the
        /// stored tokens; play the video from /files/{videoUrl}.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Courses.AdminLessons)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<LessonAdminDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddLesson(
            [FromForm] AddLessonCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update a lesson. Omitting both videoFile and videoUrl keeps the current video; sending
        /// studyBookUrl empty removes the study book, omitting the field keeps it. A new upload
        /// replaces and deletes the old file. Moving a lesson to another course is not supported;
        /// delete and recreate it instead.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Courses.AdminLessonById)]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = 4294967295L)]
        [ProducesResponseType(typeof(ApiResponse<LessonAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateLesson(
            [FromRoute] Guid id,
            [FromForm] UpdateLessonCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Soft delete a lesson. A lesson that still holds quiz questions cannot be deleted;
        /// delete the questions first.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Courses.AdminLessonById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLesson([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteLessonCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Create a quiz question on a lesson with its options sent in the same body. Between 2
        /// and 10 options, exactly one of them correct.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Courses.AdminQuizQuestions)]
        [ProducesResponseType(typeof(ApiResponse<QuizQuestionAdminDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddQuizQuestion(
            [FromBody] AddQuizQuestionCommand command,
            CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Update a quiz question. The options list replaces the current one in full, so the
        /// option identifiers change on every save.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut(ApiRoutes.Courses.AdminQuizQuestionById)]
        [ProducesResponseType(typeof(ApiResponse<QuizQuestionAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateQuizQuestion(
            [FromRoute] Guid id,
            [FromBody] UpdateQuizQuestionCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await Mediator.Send(command, cancellationToken);
            return FromResult(result);
        }

        /// <summary>
        /// Soft delete a quiz question together with its options.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete(ApiRoutes.Courses.AdminQuizQuestionById)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteQuizQuestion([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new DeleteQuizQuestionCommand { Id = id }, cancellationToken);
            return FromResult(result);
        }
    }
}
