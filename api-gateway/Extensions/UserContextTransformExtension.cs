using Yarp.ReverseProxy.Transforms;

namespace ApiGateway.Extensions;

public static class UserContextTransformExtension
{
    /// <summary>
    /// Inyecta X-User-Id y X-User-Email en cada request proxeado para que
    /// los microservicios no necesiten parsear el JWT.
    /// </summary>
    public static IReverseProxyBuilder AddUserContextTransforms(this IReverseProxyBuilder builder)
    {
        builder.AddTransforms(ctx =>
        {
            ctx.AddRequestTransform(transform =>
            {
                var user = transform.HttpContext.User;
                if (user.Identity?.IsAuthenticated != true) return ValueTask.CompletedTask;

                var userId = user.FindFirst("sub")?.Value   ?? string.Empty;
                var email  = user.FindFirst("email")?.Value ?? string.Empty;

                // Remover primero para evitar duplicados si el cliente los envía
                transform.ProxyRequest.Headers.Remove("X-User-Id");
                transform.ProxyRequest.Headers.Remove("X-User-Email");
                transform.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id",    userId);
                transform.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Email", email);

                return ValueTask.CompletedTask;
            });
        });

        return builder;
    }
}
