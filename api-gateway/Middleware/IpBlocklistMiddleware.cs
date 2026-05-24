namespace ApiGateway.Middleware;

public class IpBlocklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpBlocklistMiddleware> _logger;
    private readonly HashSet<string> _blockedIps;

    public IpBlocklistMiddleware(
        RequestDelegate next,
        ILogger<IpBlocklistMiddleware> logger,
        IConfiguration config)
    {
        _next       = next;
        _logger     = logger;
        _blockedIps = config.GetSection("Security:BlockedIps")
                           .Get<string[]>()
                           ?.ToHashSet() ?? [];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = GetClientIp(context);

        if (_blockedIps.Contains(ip))
        {
            _logger.LogWarning("IP bloqueada: {Ip} → {Path}", ip, context.Request.Path);
            context.Response.StatusCode  = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                status  = 403,
                error   = "Acceso denegado",
                message = "Tu dirección IP está bloqueada."
            }));
            return;
        }

        await _next(context);
    }

    private static string GetClientIp(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        return !string.IsNullOrEmpty(forwarded)
            ? forwarded.Split(',')[0].Trim()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
