using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGateway.Extensions;

public static class HealthChecksExtension
{
    public static IServiceCollection AddGatewayHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var urls = configuration.GetSection("Microservices");

        services
            .AddHealthChecks()
            .AddUrlGroup(new Uri($"{urls["Productos"]}/health"),
                name: "ms-productos", failureStatus: HealthStatus.Degraded,
                tags: ["microservicio", "productos"])
            .AddUrlGroup(new Uri($"{urls["Clientes"]}/health"),
                name: "ms-clientes", failureStatus: HealthStatus.Degraded,
                tags: ["microservicio", "clientes"])
            .AddUrlGroup(new Uri($"{urls["Ordenes"]}/health"),
                name: "ms-ordenes", failureStatus: HealthStatus.Degraded,
                tags: ["microservicio", "ordenes"])
            .AddUrlGroup(new Uri($"{urls["Pagos"]}/health"),
                name: "ms-pagos", failureStatus: HealthStatus.Degraded,
                tags: ["microservicio", "pagos"])
            .AddUrlGroup(new Uri($"{urls["Envios"]}/health"),
                name: "ms-envios", failureStatus: HealthStatus.Degraded,
                tags: ["microservicio", "envios"])
            .AddUrlGroup(new Uri($"{urls["Promociones"]}/health"),
                name: "ms-promociones", failureStatus: HealthStatus.Degraded,
                tags: ["microservicio", "promociones"]);

        return services;
    }
}
