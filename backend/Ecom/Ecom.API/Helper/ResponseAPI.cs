namespace Ecom.API.Helper;

/// <summary>
/// Represents a uniform API response carrying a status code and message.
/// </summary>
public class ResponseAPI
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseAPI"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="message">An optional message; defaults to the standard message for the status code.</param>
    public ResponseAPI(int statusCode, string? message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetMessageFromStatusCode(statusCode);
    }

    private static string GetMessageFromStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 => "OK",
            201 => "Created",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => "Unknown Status Code"
        };
    }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the message describing the response.
    /// </summary>
    public string? Message { get; set; }
}
