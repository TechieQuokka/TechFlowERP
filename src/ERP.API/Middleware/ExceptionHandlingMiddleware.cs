using ERP.SharedKernel.Utilities;

using System.Net;
using System.Text.Json;

namespace ERP.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                BusinessRuleViolationException businessEx => (HttpStatusCode.BadRequest, businessEx.Message),
                DomainException domainEx => (HttpStatusCode.BadRequest, domainEx.Message),
                ArgumentException argEx => (HttpStatusCode.BadRequest, argEx.Message),
                InvalidOperationException invalidEx => (HttpStatusCode.BadRequest, invalidEx.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access"),
                _ => (HttpStatusCode.InternalServerError, "An internal server error occurred")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                Message = message,
                StatusCode = (int)statusCode,
                Timestamp = DateTime.UtcNow,
                ErrorCode = GetErrorCode(exception)
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static string GetErrorCode(Exception exception)
        {
            return exception switch
            {
                BusinessRuleViolationException businessEx => businessEx.ErrorCode,
                DomainException domainEx => domainEx.ErrorCode,
                ArgumentException => "INVALID_ARGUMENT",
                InvalidOperationException => "INVALID_OPERATION",
                UnauthorizedAccessException => "UNAUTHORIZED",
                _ => "INTERNAL_ERROR"
            };
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = default!;
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}
