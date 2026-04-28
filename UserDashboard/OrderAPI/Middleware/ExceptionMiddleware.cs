
    using OrderAPI.Common;
    
    using Serilog;
    using System.Net;
    using System.Text.Json;
namespace OrderAPI.MiddleWare
{
  

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context.TraceIdentifier;

            // ── Log with Serilog (structured) ────────────────────
            Log.Error(exception,
                "Unhandled exception. TraceId: {TraceId} | Path: {Path} | Method: {Method}",
                traceId,
                context.Request.Path,
                context.Request.Method);

            // ── Map exception type → HTTP status ─────────────────
            ApiResponse<object> response = exception switch
            {
                NotFoundException ex => new()
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message,
                    TraceId = traceId
                },
                BadRequestException ex => new()
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                    Errors = ex.Errors,
                    TraceId = traceId
                },
                ConflictException ex => new()
                {
                    Success = false,
                    StatusCode = 409,
                    Message = ex.Message,
                    TraceId = traceId
                },
                ValidationException ex => new()
                {
                    Success = false,
                    StatusCode = 422,
                    Message = ex.Message,
                    Errors = ex.Errors,
                    TraceId = traceId
                },
                // Fallback — unexpected server error
                _ => new()
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "An unexpected error occurred. Please try again later.",
                    TraceId = traceId
                }
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.StatusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    /// <summary>Extension to register middleware cleanly in Program.cs</summary>
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();
    }
}
