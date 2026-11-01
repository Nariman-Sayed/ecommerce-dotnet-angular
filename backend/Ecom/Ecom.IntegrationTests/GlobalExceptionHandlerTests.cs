using System.Text.Json;
using Ecom.API.Helper;
using Ecom.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Ecom.IntegrationTests;

/// <summary>
/// Unit tests for <see cref="GlobalExceptionHandler"/>.
/// </summary>
public class GlobalExceptionHandlerTests
{
    private static GlobalExceptionHandler CreateHandler(bool isDevelopment, ILogger<GlobalExceptionHandler>? logger = null)
    {
        var environment = new FakeHostEnvironment(
            isDevelopment ? "Development" : "Testing");
        return new GlobalExceptionHandler(
            logger ?? NullLogger<GlobalExceptionHandler>.Instance,
            environment);
    }

    private static async Task<ResponseAPI> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ResponseAPI>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    [Fact]
    public async Task UnexpectedException_Development_IncludesExceptionDetails()
    {
        var exception = CaptureException(new InvalidOperationException("something broke"));
        var handler = CreateHandler(true);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, body.StatusCode);
        Assert.Contains("something broke", body.Message);
        Assert.Contains("at ", body.Message);
    }

    [Fact]
    public async Task UnexpectedException_NonDevelopment_HidesExceptionDetails()
    {
        var exception = new InvalidOperationException("something broke");
        var handler = CreateHandler(false);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var body = await ReadBodyAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, body.StatusCode);
        Assert.Equal("Internal Server Error", body.Message);
        Assert.DoesNotContain("at ", body.Message);
    }

    [Fact]
    public async Task TryHandleAsync_LogsTheException()
    {
        var exception = new InvalidOperationException("something broke");
        var logger = new RecordingLogger<GlobalExceptionHandler>();
        var handler = CreateHandler(false, logger);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(exception, entry.Exception);
    }

    private static Exception CaptureException(Exception exception)
    {
        try
        {
            throw exception;
        }
        catch (Exception captured)
        {
            return captured;
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, Exception? Exception)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, exception));
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new FakeFileProvider();
    }

    private sealed class FakeFileProvider : IFileProvider
    {
        public IFileInfo GetFileInfo(string subpath) => new FakeFileInfo();
        public IDirectoryContents GetDirectoryContents(string subpath) => new FakeDirectoryContents();
        public IChangeToken Watch(string filter) => NullChangeToken.Singleton;
    }

    private sealed class FakeFileInfo : IFileInfo
    {
        public bool Exists => false;
        public bool IsDirectory => false;
        public DateTimeOffset LastModified => default;
        public long Length => 0;
        public string Name => string.Empty;
        public string PhysicalPath => string.Empty;
        public Stream CreateReadStream() => Stream.Null;
    }

    private sealed class FakeDirectoryContents : IDirectoryContents
    {
        public bool Exists => false;
        public IEnumerator<IFileInfo> GetEnumerator() => Enumerable.Empty<IFileInfo>().GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
