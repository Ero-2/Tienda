// Tienda/ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Xml.Linq;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password = string.Empty;

    // ✅ Propiedad adicional para RegisterPage
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    // ✅ Método CanLogin actualizado para incluir Name
    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        Password.Length >= 6 &&
        Email.Contains('@');

    // ✅ Método CanRegister para el comando de registro
    private bool CanRegister() =>
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(Name) && // ✅ Requiere Nombre
        Password.Length >= 6 &&
        Email.Contains('@');

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        await Task.Delay(1200); // Simula llamada a API

        // Lógica real: reemplaza con tu AuthService
        if (Email == "demo@thedrop.mx" && Password == "drop2025")
        {
            _authService.Login(Email);
            await Shell.Current.GoToAsync("///ProductsPage");
        }
        else
        {
            ErrorMessage = "Credenciales incorrectas. Intenta de nuevo.";
        }

        IsLoading = false;
    }

    // ✅ Nuevo comando RegisterCommand
    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        await Task.Delay(1200); // Simula proceso de registro

        // Lógica de registro simulado
        if (Email.Contains("@") && Password.Length >= 6 && !string.IsNullOrWhiteSpace(Name))
        {
            // Aquí iría la lógica real de registro (API, BD, etc.)
            // Por ahora, simulamos éxito: creamos sesión y vamos a ProductsPage
            _authService.Login(Email); // Considera si quieres hacer login automático o pedir login después
            await Shell.Current.GoToAsync("///ProductsPage");
        }
        else
        {
            ErrorMessage = "Datos inválidos. Por favor, revisa nombre, correo y contraseña.";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private async Task GoToRegisterAsync() =>
        await Shell.Current.GoToAsync("RegisterPage");

    [RelayCommand]
    private async Task ContinueAsGuestAsync()
    {
        await Shell.Current.GoToAsync("///ProductsPage");
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync() =>
        await Shell.Current.DisplayAlert("THE DROP", "Te enviaremos un enlace a tu correo.", "OK");

    // ✅ Comando para volver a Login desde Register (opcional)
    [RelayCommand]
    private async Task GoToLoginAsync() =>
        await Shell.Current.GoToAsync(".."); // Vuelve a la página anterior
}