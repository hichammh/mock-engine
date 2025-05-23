namespace MockEngine.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseDynamicMockMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DynamicMockMiddleware>();
    }
}