using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Extensions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Models;
using LMS.Application.Localization;
using System.Text.Json;

namespace LMS.API.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug("Request was cancelled by the client.");
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            if (exception is OperationCanceledException)
            {
                return;
            }

            var localizationProvider = context.RequestServices.GetService<ILocalizationProvider>();
            var culture = GetRequestCulture(context);

            string Localize(string key, params object[]? args)
            {
                if (localizationProvider == null || string.IsNullOrWhiteSpace(key))
                    return key;

                return args != null && args.Length > 0
                    ? localizationProvider.GetLocalizedString(key, culture, args)
                    : localizationProvider.GetLocalizedString(key, culture);
            }

            int statusCode;
            string message;
            IDictionary<string, string[]> errorsDict = new Dictionary<string, string[]>();

            switch (exception)
            {
                case ValidationException customValEx:
                    statusCode = customValEx.StatusCode;
                    message = Localize(customValEx.LocalizationKey, customValEx.Args);
                    if (customValEx.Errors.Any())
                    {
                        foreach (var kvp in customValEx.Errors)
                        {
                            var key = string.IsNullOrWhiteSpace(kvp.Key) ? "General" : kvp.Key;
                            errorsDict[key] = kvp.Value.Select(e => Localize(e)).ToArray();
                        }
                    }
                    else
                    {
                        errorsDict["General"] = new[] { message };
                    }
                    break;

                case FluentValidation.ValidationException fvEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = Localize(LocalizationKeys.ExceptionMessages.Validation);
                    if (fvEx.Errors.Any())
                    {
                        errorsDict = fvEx.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => string.IsNullOrWhiteSpace(g.Key) ? "General" : g.Key,
                                g => g.Select(e => Localize(e.ErrorMessage)).ToArray());
                    }
                    else
                    {
                        errorsDict["General"] = new[] { message };
                    }
                    break;

                case BadRequestException badRequestEx:
                    statusCode = badRequestEx.StatusCode;
                    message = Localize(badRequestEx.LocalizationKey, badRequestEx.Args);
                    if (badRequestEx.Errors.Any())
                    {
                        foreach (var kvp in badRequestEx.Errors)
                        {
                            var key = string.IsNullOrWhiteSpace(kvp.Key) ? "General" : kvp.Key;
                            errorsDict[key] = kvp.Value.Select(e => Localize(e)).ToArray();
                        }
                    }
                    else
                    {
                        errorsDict["General"] = new[] { message };
                    }
                    break;

                case NotFoundException notFoundEx:
                    statusCode = notFoundEx.StatusCode;
                    message = Localize(notFoundEx.LocalizationKey, notFoundEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case KeyNotFoundException keyNotFoundEx:
                    statusCode = StatusCodes.Status404NotFound;
                    message = Localize(LocalizationKeys.ExceptionMessages.NotFound);
                    errorsDict["General"] = new[] { message };
                    break;

                case UnAuthorizedException unAuthEx:
                    statusCode = unAuthEx.StatusCode;
                    message = Localize(unAuthEx.LocalizationKey, unAuthEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case UnauthorizedAccessException unAuthAccessEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    _logger.LogWarning("Unauthorized access: {Message}", unAuthAccessEx.Message);
                    message = Localize(LocalizationKeys.ExceptionMessages.Unauthorized);
                    errorsDict["General"] = new[] { message };
                    break;

                case ForbiddenException forbiddenEx:
                    statusCode = forbiddenEx.StatusCode;
                    message = Localize(forbiddenEx.LocalizationKey, forbiddenEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case ConflictException conflictEx:
                    statusCode = conflictEx.StatusCode;
                    message = Localize(conflictEx.LocalizationKey, conflictEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case TooManyRequestsException tooManyRequestsEx:
                    statusCode = tooManyRequestsEx.StatusCode;
                    message = Localize(tooManyRequestsEx.LocalizationKey, tooManyRequestsEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case PayloadTooLargeException payloadTooLargeEx:
                    statusCode = payloadTooLargeEx.StatusCode;
                    message = Localize(payloadTooLargeEx.LocalizationKey, payloadTooLargeEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case UnprocessableEntityException unprocessableEntityEx:
                    statusCode = unprocessableEntityEx.StatusCode;
                    message = Localize(unprocessableEntityEx.LocalizationKey, unprocessableEntityEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case ServiceUnavailableException serviceUnavailableEx:
                    statusCode = serviceUnavailableEx.StatusCode;
                    message = Localize(serviceUnavailableEx.LocalizationKey, serviceUnavailableEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case NotAcceptableException notAcceptableEx:
                    statusCode = notAcceptableEx.StatusCode;
                    message = Localize(notAcceptableEx.LocalizationKey, notAcceptableEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case GoneException goneEx:
                    statusCode = goneEx.StatusCode;
                    message = Localize(goneEx.LocalizationKey, goneEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case MethodNotAllowedException methodNotAllowedEx:
                    statusCode = methodNotAllowedEx.StatusCode;
                    message = Localize(methodNotAllowedEx.LocalizationKey, methodNotAllowedEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case UnsupportedMediaTypeException unsupportedMediaTypeEx:
                    statusCode = unsupportedMediaTypeEx.StatusCode;
                    message = Localize(unsupportedMediaTypeEx.LocalizationKey, unsupportedMediaTypeEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case RequestTimeoutException requestTimeoutEx:
                    statusCode = requestTimeoutEx.StatusCode;
                    message = Localize(requestTimeoutEx.LocalizationKey, requestTimeoutEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case ArgumentException argEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = Localize(LocalizationKeys.ExceptionMessages.BadRequest);
                    errorsDict["General"] = new[] { message };
                    break;

                case DomainException domainEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    var domainKey = !string.IsNullOrWhiteSpace(domainEx.LocalizationKey) ? domainEx.LocalizationKey : LocalizationKeys.ExceptionMessages.BadRequest;
                    message = Localize(domainKey, domainEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case ILocalizedException localizedEx:
                    statusCode = localizedEx.StatusCode;
                    message = Localize(localizedEx.LocalizationKey, localizedEx.Args);
                    errorsDict["General"] = new[] { message };
                    break;

                case MailKit.Security.AuthenticationException:
                case MailKit.Net.Smtp.SmtpProtocolException:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = Localize(LocalizationKeys.ExceptionMessages.EmailAuthenticationFailed);
                    errorsDict["General"] = new[] { message };
                    break;

                case MailKit.Net.Smtp.SmtpCommandException:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = Localize(LocalizationKeys.ExceptionMessages.EmailServerRejected);
                    errorsDict["General"] = new[] { message };
                    break;

                case System.Net.Sockets.SocketException:
                    statusCode = StatusCodes.Status503ServiceUnavailable;
                    message = Localize(LocalizationKeys.ExceptionMessages.NetworkConnectionFailed);
                    errorsDict["General"] = new[] { message };
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = Localize(LocalizationKeys.ExceptionMessages.InternalServerError);
                    errorsDict["General"] = new[] { message };
                    break;
            }

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = statusCode;

            var response = ApiResponse<object?>.Error(errorsDict, message, statusCode);

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions), System.Text.Encoding.UTF8);
        }

        private static string GetRequestCulture(HttpContext context)
        {
            var req = context?.Request;
            if (req != null)
            {
                var headers = req.Headers;
                var hCulture = headers["Accept-Language"].FirstOrDefault()
                               ?? headers["Language"].FirstOrDefault()
                               ?? headers["language"].FirstOrDefault()
                               ?? headers["Culture"].FirstOrDefault()
                               ?? headers["Lang"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(hCulture))
                {
                    return AppLanguageExtensions.FromCode(hCulture).ToCode();
                }

                var qCulture = req.Query["culture"].FirstOrDefault()
                               ?? req.Query["lang"].FirstOrDefault()
                               ?? req.Query["language"].FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(qCulture))
                {
                    return AppLanguageExtensions.FromCode(qCulture).ToCode();
                }
            }

            var current = System.Globalization.CultureInfo.CurrentUICulture?.Name;
            return AppLanguageExtensions.FromCode(current).ToCode();
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}
