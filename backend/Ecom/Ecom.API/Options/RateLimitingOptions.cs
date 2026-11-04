using System.ComponentModel.DataAnnotations;

namespace Ecom.API.Options;

/// <summary>
/// Strongly typed rate limiting configuration bound from the <c>RateLimiting</c> section.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Gets or sets the permit limit per window.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; } = 8;

    /// <summary>
    /// Gets or sets the window size in seconds.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int WindowSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the queue limit.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int QueueLimit { get; set; }
}
