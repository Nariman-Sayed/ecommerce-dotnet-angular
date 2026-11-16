using Ecom.API.Helper;
using Microsoft.AspNetCore.Mvc;

namespace Ecom.API.Controllers;

/// <summary>
/// Serves the standard error response for re-executed status code pages.
/// </summary>
[Route("errors/{statusCode}")]
[ApiController]
public class ErrorController : ControllerBase
{
    /// <summary>
    /// Returns a uniform error response for the given HTTP status code,
    /// including status code pages re-executed from non-GET requests.
    /// </summary>
    /// <param name="statusCode">The HTTP status code that triggered the re-execution.</param>
    /// <returns>An object result carrying the status code and its standard message.</returns>
    [AcceptVerbs("GET", "POST", "PUT", "PATCH", "DELETE")]
    public IActionResult Error(int statusCode)
    {
        return new ObjectResult(new ResponseAPI(statusCode));
    }
}
