using Microsoft.AspNetCore.Identity;

namespace LMS.Application.Common.Models
{
    public class Result
    {
        public const string GeneralErrorsKey = "General";

        public bool Succeeded { get; set; }
        public object? MainResult { get; set; }
        public object? Data { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }

        internal Result(bool succeeded, object? mainResult, object? data, IDictionary<string, string[]> errors)
        {
            MainResult = mainResult;
            Succeeded = succeeded;
            Errors = errors;
            Data = data;
        }
    }

    public static class IdentityResultMap
    {
        private static IDictionary<string, string[]> BuildErrors(IEnumerable<string> errors)
        {
            var dictionary = new Dictionary<string, string[]>();
            if (errors is not null)
            {
                var all = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
                if (all.Length > 0)
                    dictionary[Result.GeneralErrorsKey] = all;
            }

            return dictionary;
        }

        public static Result Success(this IdentityResult mainResult, object data)
        {
            return new Result(true, mainResult, data, new Dictionary<string, string[]>());
        }

        public static Result Success(this IdentityResult mainResult)
        {
            return new Result(true, mainResult, null, new Dictionary<string, string[]>());
        }

        public static Result Failure(this IdentityResult mainResult, IEnumerable<string> errors)
        {
            return new Result(false, mainResult, null, BuildErrors(errors));
        }

        public static Result Failure(this IdentityResult mainResult, IDictionary<string, string[]> errors)
        {
            return new Result(false, mainResult, null, errors ?? new Dictionary<string, string[]>());
        }
    }

    public static class SginInResultMap
    {
        private static IDictionary<string, string[]> BuildErrors(IEnumerable<string> errors)
        {
            var dictionary = new Dictionary<string, string[]>();
            if (errors is not null)
            {
                var all = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
                if (all.Length > 0)
                    dictionary[Result.GeneralErrorsKey] = all;
            }

            return dictionary;
        }

        public static Result Success(this SignInResult mainResult, object data)
        {
            return new Result(true, mainResult, data, new Dictionary<string, string[]>());
        }

        public static Result Success(this SignInResult mainResult)
        {
            return new Result(true, mainResult, null, new Dictionary<string, string[]>());
        }

        public static Result Failure(this SignInResult mainResult, IEnumerable<string> errors)
        {
            return new Result(false, mainResult, null, BuildErrors(errors));
        }

        public static Result Failure(this SignInResult mainResult, IDictionary<string, string[]> errors)
        {
            return new Result(false, mainResult, null, errors ?? new Dictionary<string, string[]>());
        }
    }

    public class Result<T>
    {
        public const string GeneralErrorsKey = Result.GeneralErrorsKey;

        public bool Succeeded { get; set; }
        public bool IsSuccess => Succeeded;
        public int StatusCode { get; set; } = 200;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        public Result() { }

        public Result(bool succeeded, int statusCode, string message, T? data, IDictionary<string, string[]>? errors = null)
        {
            Succeeded = succeeded;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        public static Result<T> Success(T data)
            => new(true, 200, string.Empty, data);

        public static Result<T> Success(T data, string message, int statusCode = 200)
            => new(true, statusCode, message, data);

        public static Result<T> Failure(IDictionary<string, string[]> errors, string message, int statusCode = 400)
            => new(false, statusCode, message, default, errors);

        public static Result<T> Failure(string message, int statusCode, params string[] errors)
        {
            var all = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
            if (all.Length == 0)
                return new(false, statusCode, message, default, BuildGeneralErrors(message));

            return new(false, statusCode, message, default, new Dictionary<string, string[]>
            {
                [GeneralErrorsKey] = all
            });
        }

        public static Result<T> Failure(string field, string message, int statusCode = 400)
            => new(false, statusCode, message, default, new Dictionary<string, string[]>
            {
                [string.IsNullOrWhiteSpace(field) ? GeneralErrorsKey : field] = new[] { message }
            });

        public static Result<T> Failure(IDictionary<string, string[]> errors, int statusCode = 400, string? message = null)
            => new(false, statusCode, message ?? string.Join(" ", errors.SelectMany(e => e.Value)), default, errors);

        private static IDictionary<string, string[]> BuildGeneralErrors(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return new Dictionary<string, string[]>();

            return new Dictionary<string, string[]> { [GeneralErrorsKey] = new[] { message } };
        }

        public ApiResponse<T> ToApiResponse()
        {
            if (Succeeded)
                return ApiResponse<T>.Ok(Data, Message, StatusCode);

            var errors = Errors.Count > 0
                ? Errors
                : BuildGeneralErrors(Message);

            return ApiResponse<T>.Error(errors, Message, StatusCode);
        }
    }

    public static class ResultExtensions
    {
        public static Result<T> ToResult<T>(this T data) => Result<T>.Success(data);
    }
}
