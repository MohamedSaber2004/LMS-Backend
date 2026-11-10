using Asp.Versioning;
using LMS.API.Filters;
using LMS.API.Routes.V1;
using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Application.Features.InstructorProfile.Queries.GetTeacherLearners;
using LMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers.V1
{
    /// <summary>
    /// [Allowed Roles: Instructor (Teacher)]
    /// Teacher management of assigned students and learners.
    /// </summary>
    [ApiVersion("1.0")]
    [Route(ApiRoutes.Teacher.Students.Base)]
    [Tags("Teacher: Students")]
    [Authorize]
    [RoleAuthorize(UserType.Instructor)]
    public class TeacherStudentsController : BaseController
    {
        /// <summary>
        /// List students who have booked sessions or purchased packages with the calling teacher.
        /// </summary>
        [HttpGet(ApiRoutes.Teacher.Students.GetAll)]
        [ProducesResponseType(typeof(ApiResponse<PagginatedResult<TeacherLearnerItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyStudents([FromQuery] GetTeacherLearnersQuery query, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(query, cancellationToken);
            return FromResult(result);
        }
    }
}
