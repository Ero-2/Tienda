// Tienda/Services/IAuthService.cs
namespace Tienda.Services;

public interface IAuthService
{
    bool IsLoggedIn { get; }

    // Login/registro reales contra ms-clientes
    Task<(bool Success, string? Error)> LoginAsync(string email, string password);
    Task<(bool Success, string? Error)> RegisterAsync(string nombre, string email, string password);

    void Logout();
    string? GetUserEmail();
    string? GetToken();
    int     GetUserId();
}
