using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace EcommerceAPI.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = httpContext.TraceIdentifier;
            var path = httpContext.Request.Path.ToString();
            var method = httpContext.Request.Method;
            var userId = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity!.Name
                : "anonymous";

            var problemDetails = new CustomProblemDetails
            {
                Instance = path,
                TraceId = traceId
            };

            if (exception is AppException appEx)
            {
                problemDetails.Status = appEx.StatusCode;
                problemDetails.Title = appEx.Title;
                problemDetails.Detail = appEx.Message;

                if (appEx is ValidationException validationEx)
                {
                    problemDetails.Errors = validationEx.Errors
                        .Select(e => new ErrorDetail { Property = e.Property, Error = e.Error })
                        .ToList();
                }

                LogAppException(appEx, traceId, method, path, userId);
            }
            else
            {
                switch (exception)
                {
                    case UnauthorizedAccessException:
                        problemDetails.Status = StatusCodes.Status401Unauthorized;
                        problemDetails.Title = "Unauthorized";
                        problemDetails.Detail = "You are not authorized to perform this action.";
                        _logger.LogWarning(exception,
                            "Unauthorized access | TraceId: {TraceId} | {Method} {Path} | User: {UserId} | Type: {ExceptionType}",
                            traceId, method, path, userId, exception.GetType().Name);
                        break;

                    case KeyNotFoundException:
                        problemDetails.Status = StatusCodes.Status404NotFound;
                        problemDetails.Title = "Resource not found";
                        problemDetails.Detail = exception.Message;
                        _logger.LogInformation(
                            "Resource not found | TraceId: {TraceId} | {Method} {Path} | User: {UserId} | Type: {ExceptionType}",
                            traceId, method, path, userId, exception.GetType().Name);
                        break;

                    case OperationCanceledException:
                        problemDetails.Status = StatusCodes.Status499ClientClosedRequest;
                        problemDetails.Title = "Request cancelled";
                        problemDetails.Detail = "The request was cancelled.";
                        _logger.LogInformation(
                            "Request cancelled by client | TraceId: {TraceId} | {Method} {Path} | User: {UserId}",
                            traceId, method, path, userId);
                        break;

                    default:
                        problemDetails.Status = StatusCodes.Status500InternalServerError;
                        problemDetails.Title = "An unexpected error occurred";
                        problemDetails.Detail = "An unexpected error occurred. Please try again later.";
                        _logger.LogError(exception,
                            "UNHANDLED exception | TraceId: {TraceId} | {Method} {Path} | User: {UserId} | Type: {ExceptionType}",
                            traceId, method, path, userId, exception.GetType().Name);
                        break;
                }
            }

            httpContext.Response.StatusCode = problemDetails.Status;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private void LogAppException(AppException appEx, string traceId, string method, string path, string? userId)
        {
            var exceptionType = appEx.GetType().Name;

            if (appEx.StatusCode >= 500)
            {
                _logger.LogError(appEx,
                    "AppException with 5xx status | TraceId: {TraceId} | {Method} {Path} | User: {UserId} | Type: {ExceptionType} | Title: {Title}",
                    traceId, method, path, userId, exceptionType, appEx.Title);
            }
            else if (appEx.StatusCode == StatusCodes.Status403Forbidden || appEx.StatusCode == StatusCodes.Status401Unauthorized)
            {
                _logger.LogWarning(
                    "Access denied | TraceId: {TraceId} | {Method} {Path} | User: {UserId} | Type: {ExceptionType} | Title: {Title}",
                    traceId, method, path, userId, exceptionType, appEx.Title);
            }
            else
            {
                _logger.LogWarning(
                    "Client error | TraceId: {TraceId} | {Method} {Path} | User: {UserId} | Type: {ExceptionType} | Title: {Title}",
                    traceId, method, path, userId, exceptionType, appEx.Title);
            }
        }
    }
}