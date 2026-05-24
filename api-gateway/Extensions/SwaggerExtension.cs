using Microsoft.OpenApi.Models;

namespace ApiGateway.Extensions;

public static class SwaggerExtension
{
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "Tienda Departamental — API Gateway",
                Version     = "v1",
                Description = """
                    Punto de entrada único para todos los microservicios.

                    **Rutas disponibles:**
                    - `GET  /api/productos`              → ms-productos (:5003) — catálogo
                    - `POST /api/clientes/login`         → ms-clientes (:5004) — autenticación
                    - `POST /api/clientes/registro`      → ms-clientes (:5004) — registro
                    - `GET  /api/clientes/{id}/...`      → ms-clientes (:5004) — perfil/direcciones
                    - `*    /api/ordenes`                → ms-ordenes (:5001) — órdenes
                    - `*    /api/pagos`                  → ms-pagos (:5002) — pagos OpenPay
                    - `*    /api/envios`                 → ms-envios (:5005) — rastreo
                    - `GET  /api/promociones`            → ms-promociones (:5006) — promos activas
                    - `POST /api/promociones/calcular`   → ms-promociones (:5006) — descuento

                    **Autenticación:** Bearer JWT
                    """
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.Http,
                Scheme      = "Bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "Ingresa el token JWT. Ejemplo: Bearer eyJhbGci..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static WebApplication UseGatewaySwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Tienda — API Gateway";
        });
        return app;
    }
}
