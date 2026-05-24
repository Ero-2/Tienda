using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace ApiGateway.Extensions;

public static class RateLimitingExtension
{
    public const string PublicPolicy        = "public";
    public const string AuthenticatedPolicy = "authenticated";
    public const string StrictPolicy        = "strict";

    public static IServiceCollection AddGatewayRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.Headers["Retry-After"] = "10";
                await context.HttpContext.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status  = 429,
                        error   = "Demasiadas solicitudes",
                        message = "Excediste el límite de peticiones. Intenta en unos segundos.",
                        retryAfterSeconds = 10
                    }), cancellationToken);
            };

            // 30 req/min por IP — invitados y rutas públicas
            options.AddFixedWindowLimiter(PublicPolicy, opt =>
            {
                opt.PermitLimit          = 30;
                opt.Window               = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit           = 5;
            });

            // 120 req/min por usuario autenticado — sliding window
            options.AddSlidingWindowLimiter(AuthenticatedPolicy, opt =>
            {
                opt.PermitLimit          = 120;
                opt.Window               = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow    = 6;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit           = 10;
            });

            // 10 req/min por IP — login, registro, pagos
            options.AddFixedWindowLimiter(StrictPolicy, opt =>
            {
                opt.PermitLimit          = 10;
                opt.Window               = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit           = 0;
            });

            // Límite global de concurrencia: 20 simultáneos por usuario/IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext =>
                {
                    var userId = httpContext.User?.FindFirst("sub")?.Value;
                    var key    = userId ?? GetClientIp(httpContext);
                    return RateLimitPartition.GetConcurrencyLimiter(key, _ =>
                        new ConcurrencyLimiterOptions
                        {
                            PermitLimit          = 20,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit           = 5
                        });
                });
        });

        return services;
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
            return forwardedFor.Split(',')[0].Trim();
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}