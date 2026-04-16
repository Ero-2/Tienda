// Tienda/Services/IAuthService.cs
namespace Tienda.Services;

public interface IAuthService
{
    bool IsLoggedIn { get; }
    void Login(string email);
    void Logout();
    string? GetUserEmail();
}