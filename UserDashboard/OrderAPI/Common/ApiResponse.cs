namespace OrderAPI.Common
{

    /// <summary>
    /// Universal API response envelope.
    /// Every endpoint returns ApiResponse{T} so clients always
    /// know exactly what shape to expect.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string TraceId { get; set; } = string.Empty;

        // ── Factory methods ───────────────────────────────────

        public static ApiResponse<T> Ok(T data, string message = "Success") => new()
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data
        };

        public static ApiResponse<T> Created(T data, string message = "Created successfully") => new()
        {
            Success = true,
            StatusCode = 201,
            Message = message,
            Data = data
        };

        public static ApiResponse<T> Fail(int statusCode, string message, List<string>? errors = null) => new()
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors ?? new()
        };

        public static ApiResponse<T> NotFound(string message = "Resource not found") =>
            Fail(404, message);

        public static ApiResponse<T> BadRequest(string message, List<string>? errors = null) =>
            Fail(400, message, errors);

        public static ApiResponse<T> ServerError(string message = "An unexpected error occurred") =>
            Fail(500, message);
    }

    /// <summary>Non-generic shorthand for void/empty responses.</summary>
    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse OkEmpty(string message = "Success") => new()
        {
            Success = true,
            StatusCode = 200,
            Message = message
        };
    }
}
