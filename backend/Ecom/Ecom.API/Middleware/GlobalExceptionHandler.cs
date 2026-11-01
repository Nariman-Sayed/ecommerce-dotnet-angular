using Ecom.API.Helper;
using Microsoft.AspNetCore.Diagnostics;

namespace Ecom.API.Middleware;

/// <summary>
/// Logs unhandled exceptions and writes a uniform <see cref="ResponseAPI"/> response.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record exceptions.</param>
    /// <param name="environment">The hosting environment.</param>
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Domain guards throw ArgumentException for invalid client input. Every
        // throw site in this codebase is an entity guard, so a 400 is safe.
        var isInvalidInput = exception is ArgumentException;

        if (isInvalidInput)
        {
            _logger.LogWarning(
                exception,
                "Invalid request for {Path}. TraceId: {TraceId}",
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Path}. TraceId: {TraceId}",
                httpContext.Request.Path,
                httpContext.TraceIdentifier);
        }

        var statusCode = isInvalidInput
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        // Validation messages are safe to return; other exception details only
        // leave the process in Development.
        var message = _environment.IsDevelopment()
            ? exception.ToString()
            : isInvalidInput ? exception.Message : null;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new ResponseAPI(statusCode, message),
            cancellationToken);

        return true;
    }
}
