using Microsoft.Extensions.Caching.Memory;

namespace ApiGateway.Middleware;

public class ResponseCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ResponseCacheMiddleware> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public ResponseCacheMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<ResponseCacheMiddleware> logger)
    {
        _next   = next;
        _cache  = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsCacheable(context.Request))
        {
            await _next(context);
            return;
        }

        var key = $"gw:{context.Request.Path}{context.Request.QueryString}";

        if (_cache.TryGetValue(key, out CachedResponse? hit) && hit is not null)
        {
            _logger.LogDebug("Cache HIT: {Key}", key);
            context.Response.StatusCode  = hit.StatusCode;
            context.Response.ContentType = hit.ContentType;
            context.Response.Headers["X-Cache"] = "HIT";
            await context.Response.Body.WriteAsync(hit.Body);
            return;
        }

        var original = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        var body = buffer.ToArray();

        if (context.Response.StatusCode == 200 && body.Length > 0)
        {
            _cache.Set(key, new CachedResponse
            {
                StatusCode  = context.Response.StatusCode,
                ContentType = context.Response.ContentType ?? "application/json",
                Body        = body
            }, CacheDuration);
            context.Response.Headers["X-Cache"] = "MISS";
            _logger.LogDebug("Cache MISS (guardado): {Key}", key);
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(original);
        context.Response.Body = original;
    }

    private static bool IsCacheable(HttpRequest request) =>
        request.Method == HttpMethods.Get &&
        request.Path.StartsWithSegments("/api/productos");

    private sealed record CachedResponse
    {
        public int    StatusCode  { get; init; }
        public string ContentType { get; init; } = string.Empty;
        public byte[] Body        { get; init; } = [];
    }
}
