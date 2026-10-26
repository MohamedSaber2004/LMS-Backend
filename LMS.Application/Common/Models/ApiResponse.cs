namespace LMS.Application.Common.Models
{
    public record ApiResponse<TData>
    {
        public bool Success { get; set; }

        public bool IsSuccess => Success;

        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        public TData? Data { get; set; }

        public string? Message { get; set; }

        public int StatusCode { get; set; }

        public static ApiResponse<TData> Ok(TData? data, string? message = null, int statusCode = 200)
        {
            return new ApiResponse<TData>
            {
                Success = true,
                Errors = new Dictionary<string, string[]>(),
                Data = data,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<TData> Error(IDictionary<string, string[]>? errors = default, string? message = null, int statusCode = 400)
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Errors = errors ?? new Dictionary<string, string[]>(),
                Data = default,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<TData> Error(string? message = null, int statusCode = 400)
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Errors = new Dictionary<string, string[]>(),
                Data = default,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<TData> Error(string message, int statusCode, IEnumerable<string> errors)
        {
            return new ApiResponse<TData>
            {
                Success = false,
                Errors = new Dictionary<string, string[]> { { "General", errors.ToArray() } },
                Data = default,
                Message = message,
                StatusCode = statusCode
            };
        }
    }

    public record ApiResponse : ApiResponse<object?>
    {
        public static ApiResponse Ok(string? message = null, int statusCode = 200)
        {
            return new ApiResponse
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = null,
                Errors = new Dictionary<string, string[]>()
            };
        }

        public static ApiResponse<T> Ok<T>(T? data, string? message = null, int statusCode = 200)
            => ApiResponse<T>.Ok(data, message, statusCode);

        public static new ApiResponse Error(string? message = null, int statusCode = 400)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = null,
                Errors = new Dictionary<string, string[]>()
            };
        }

        public static new ApiResponse Error(IDictionary<string, string[]>? errors, string? message = null, int statusCode = 400)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = null,
                Errors = errors ?? new Dictionary<string, string[]>()
            };
        }
    }
}
