using System.Text.Json;
using MockEngine.Interfaces;

namespace MockEngine.Middleware;

public class DynamicMockMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DynamicMockMiddleware> _logger;
    private readonly IMockStore _mockStore;

    public DynamicMockMiddleware(
        RequestDelegate next,
        ILogger<DynamicMockMiddleware> logger,
        IMockStore mockStore)
    {
        _next = next;
        _logger = logger;
        _mockStore = mockStore;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip API controller paths
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value?.ToLowerInvariant();
        var method = context.Request.Method;

        _logger.LogInformation("Received request: {Method} {Path}", method, path);

        if (string.IsNullOrEmpty(path))
        {
            await _next(context);
            return;
        }

        // Try to find a matching mock endpoint
        var mockEndpoint = await _mockStore.GetEndpointAsync(path, method);
        
        if (mockEndpoint == null)
        {
            _logger.LogInformation("No mock found for {Method} {Path}", method, path);
            await _next(context);
            return;
        }

        _logger.LogInformation("Mock found for {Method} {Path}, returning status {StatusCode}", 
            method, path, mockEndpoint.StatusCode);

        // Add any custom headers
        foreach (var header in mockEndpoint.Headers)
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        // Set the status code
        context.Response.StatusCode = mockEndpoint.StatusCode;

        // Add simulated delay if specified
        if (mockEndpoint.DelayMs > 0)
        {
            await Task.Delay(mockEndpoint.DelayMs);
        }

        // Write the response body if available
        if (mockEndpoint.ResponseBody != null)
        {
            context.Response.ContentType = "application/json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            await context.Response.WriteAsJsonAsync(mockEndpoint.ResponseBody, options);
        }
    }
}