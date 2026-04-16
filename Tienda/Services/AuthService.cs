// Tienda/Services/AuthService.cs
using Microsoft.Maui.Storage;

namespace Tienda.Services;

public class AuthService : IAuthService
{
    private const string IsLoggedInKey = "isLoggedIn";
    private const string UserEmailKey = "userEmail";

    public bool IsLoggedIn => Preferences.Get(IsLoggedInKey, false);

    public void Login(string email)
    {
        Preferences.Set(IsLoggedInKey, true);
        Preferences.Set(UserEmailKey, email);
    }

    public void Logout()
    {
        Preferences.Remove(IsLoggedInKey);
        Preferences.Remove(UserEmailKey);
    }

    public string? GetUserEmail() => Preferences.Get(UserEmailKey, string.Empty);
}