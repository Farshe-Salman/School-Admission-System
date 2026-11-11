using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Business.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SchoolAdmission.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, message) = exception switch
        {
            ArgumentException =>
                (StatusCodes.Status400BadRequest,
                 "Invalid Request",
                 "Please provide valid information."),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound,
                 "Resource Not Found",
                 "The requested resource was not found."),

            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden,
                 "Forbidden",
                 "You are not authorized to perform this action."),

            ConflictException ex =>
            (StatusCodes.Status409Conflict,
            "Request Conflict",
            ex.Message),

            DbUpdateException ex
    when ex.GetBaseException() is PostgresException pgEx
         && pgEx.SqlState == PostgresErrorCodes.UniqueViolation =>
    (
        StatusCodes.Status409Conflict,
        "Request Conflict",
        "You already have an active application. You cannot apply for another class."
    ),

            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Internal Server Error",
                 "Something went wrong. Please try again later.")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "An unexpected error occurred. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Request failed with status code {StatusCode}. Reason: {Message}",
                statusCode,
                exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
