namespace ApiGateway.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        var userId       = context.User?.FindFirst("sub")?.Value ?? "anonymous";
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var ip           = GetClientIp(context);

        _logger.LogInformation(
            "AUDIT | {Method} {Path} | Status:{StatusCode} | User:{UserId} | IP:{Ip} | {Duration}ms | Corr:{CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            userId,
            ip,
            sw.ElapsedMilliseconds,
            correlationId);
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrEmpty(forwarded)
            ? forwarded.Split(',')[0].Trim()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
