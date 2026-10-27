using LMS.Application.Common.Models;
using LMS.Application.Localization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace LMS.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        private IMediator? _mediator;
        private IStringLocalizer<Messages>? _localizer;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected IStringLocalizer<Messages> Localizer =>
            _localizer ??= HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();

        protected string Localize(string localizationKey, params object[] args) =>
            Localizer[localizationKey, args].Value;

        protected ActionResult FromResult<T>(Result<T>? result)
        {
            if (result is null)
                return InternalError<T>(LocalizationKeys.ExceptionMessages.UnknownException);

            var apiResponse = result.ToApiResponse();
            return StatusCode(apiResponse.StatusCode, apiResponse);
        }

        protected ActionResult FromResult<T>(ApiResponse<T>? result)
        {
            if (result is null)
                return InternalError<T>(LocalizationKeys.ExceptionMessages.UnknownException);

            return StatusCode(result.StatusCode, result);
        }

        protected ActionResult OkResult<T>(T? data, string localizationKey, params object[] args) =>
            StatusCode(
                StatusCodes.Status200OK,
                ApiResponse<T>.Ok(data, Localize(localizationKey, args)));

        protected ActionResult CreatedResult<T>(T? data, string localizationKey, params object[] args) =>
            StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<T>.Ok(data, Localize(localizationKey, args), StatusCodes.Status201Created));

        protected ActionResult AcceptedResult<T>(T? data, string localizationKey, params object[] args) =>
            StatusCode(
                StatusCodes.Status202Accepted,
                ApiResponse<T>.Ok(data, Localize(localizationKey, args), StatusCodes.Status202Accepted));

        protected ActionResult NoContentResult() => NoContent();

        protected ActionResult NotFoundResult<T>(
            string localizationKey,
            IDictionary<string, string[]>? errors = null,
            params object[] args) =>
            StatusCode(
                StatusCodes.Status404NotFound,
                ApiResponse<T>.Error(errors, Localize(localizationKey, args), StatusCodes.Status404NotFound));

        private ActionResult InternalError<T>(string localizationKey, params object[] args) =>
            StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<T>.Error(Localize(localizationKey, args), StatusCodes.Status500InternalServerError));
    }

}
