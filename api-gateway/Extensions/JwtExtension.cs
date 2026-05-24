using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiGateway.Extensions;

public static class JwtExtension
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey  = jwtSection["SecretKey"]  ?? throw new InvalidOperationException("JWT SecretKey no configurada.");
        var issuer     = jwtSection["Issuer"]      ?? throw new InvalidOperationException("JWT Issuer no configurado.");
        var audience   = jwtSection["Audience"]    ?? throw new InvalidOperationException("JWT Audience no configurado.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken            = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                    ValidateIssuer           = true,
                    ValidIssuer              = issuer,
                    ValidateAudience         = true,
                    ValidAudience            = audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero,
                    RequireExpirationTime    = true,
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("JWT falló: {Error} | Path: {Path}",
                            context.Exception.Message, context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        var userId = context.Principal?.FindFirst("sub")?.Value ?? "unknown";
                        logger.LogInformation("JWT válido | Usuario: {UserId} | Path: {Path}",
                            userId, context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status  = 401,
                            error   = "No autorizado",
                            message = "Token JWT inválido, expirado o ausente.",
                            path    = context.Request.Path.Value
                        });
                        return context.Response.WriteAsync(result);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode  = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status  = 403,
                            error   = "Prohibido",
                            message = "No tienes permisos para acceder a este recurso.",
                            path    = context.Request.Path.Value
                        });
                        return context.Response.WriteAsync(result);
                    }
                };
            });

        return services;
    }
}