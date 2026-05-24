using ApiGateway.Extensions;
using ApiGateway.Middleware;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando API Gateway...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .WriteTo.File("logs/gateway-.log",
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 7));

    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddGatewayRateLimiting();
    builder.Services.AddGatewayCors(builder.Configuration);
    builder.Services.AddGatewayHealthChecks(builder.Configuration);

    builder.Services
        .AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AuthenticatedUser", policy =>
            policy.RequireAuthenticatedUser());

        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} → {StatusCode} en {Elapsed:0.0}ms";
    });

    app.UseCors("GatewayCors");
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapReverseProxy();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminó inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}