using System.Net;
using System.Text.Json;

namespace ApiGateway.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ErrorHandlingMiddleware(RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
            await HandleStatusCodeAsync(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

        _logger.LogError(ex, "Excepción no manejada | Path: {Path} | CorrelationId: {CorrelationId}",
            context.Request.Path, correlationId);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

        var response = new GatewayErrorResponse
        {
            Status        = 500,
            Error         = "Error interno del servidor",
            Message       = _env.IsDevelopment() ? ex.Message : "Ocurrió un error inesperado.",
            Detail        = _env.IsDevelopment() ? ex.StackTrace : null,
            CorrelationId = correlationId,
            Path          = context.Request.Path,
            Timestamp     = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static async Task HandleStatusCodeAsync(HttpContext context)
    {
        if (context.Response.HasStarted || context.Response.StatusCode < 400)
            return;

        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        context.Response.ContentType = "application/json";

        var (error, message) = context.Response.StatusCode switch
        {
            404 => ("Recurso no encontrado",  "La ruta solicitada no existe en el Gateway."),
            405 => ("Método no permitido",    "El método HTTP no está soportado en esta ruta."),
            503 => ("Servicio no disponible", "El microservicio destino no responde."),
            504 => ("Timeout del gateway",    "El microservicio tardó demasiado en responder."),
            _   => ("Error del cliente",      "Solicitud inválida.")
        };

        var response = new GatewayErrorResponse
        {
            Status        = context.Response.StatusCode,
            Error         = error,
            Message       = message,
            CorrelationId = correlationId,
            Path          = context.Request.Path,
            Timestamp     = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}

public record GatewayErrorResponse
{
    public int      Status        { get; init; }
    public string   Error         { get; init; } = string.Empty;
    public string   Message       { get; init; } = string.Empty;
    public string?  Detail        { get; set; }
    public string   CorrelationId { get; init; } = string.Empty;
    public string   Path          { get; init; } = string.Empty;
    public DateTime Timestamp     { get; init; }
}