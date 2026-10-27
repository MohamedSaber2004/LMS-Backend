using LMS.Application.Common.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace LMS.API.Filters
{
    public class RequestSizeLimitFilter : IAsyncActionFilter
    {
        private readonly RequestLimitsOptions _options;

        public RequestSizeLimitFilter(IOptions<RequestLimitsOptions> options)
        {
            _options = options.Value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_options.Enabled && context.HttpContext.Request.ContentLength > _options.MaxRequestSizeBytes)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status413PayloadTooLarge);
                return;
            }

            await next();
        }
    }
}
