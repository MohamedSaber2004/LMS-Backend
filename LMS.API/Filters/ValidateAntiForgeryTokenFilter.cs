using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace LMS.API.Filters
{
    public class ValidateAntiForgeryTokenFilter : IAsyncActionFilter
    {
        private readonly IAntiforgery _antiforgery;
        private readonly AntiforgeryOptions _options;

        public ValidateAntiForgeryTokenFilter(IAntiforgery antiforgery, IOptions<AntiforgeryOptions> options)
        {
            _antiforgery = antiforgery;
            _options = options.Value;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
                !context.HttpContext.Request.Method.Equals("HEAD", StringComparison.OrdinalIgnoreCase) &&
                !context.HttpContext.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase) &&
                !context.HttpContext.Request.Method.Equals("TRACE", StringComparison.OrdinalIgnoreCase))
            {
                var request = context.HttpContext.Request;

                if (!string.IsNullOrEmpty(_options.HeaderName) && request.Headers.TryGetValue(_options.HeaderName, out _))
                {
                    try
                    {
                        await _antiforgery.ValidateRequestAsync(context.HttpContext);
                    }
                    catch (Exception)
                    {
                        context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
                        return;
                    }
                }
                else
                {
                    var hasValidToken = await _antiforgery.IsRequestValidAsync(context.HttpContext);
                    if (!hasValidToken)
                    {
                        context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
                        return;
                    }
                }
            }

            await next();
        }
    }
}
