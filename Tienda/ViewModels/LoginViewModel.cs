// Tienda/ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string name = string.Empty;

    [ObservableProperty] private bool   isLoading;
    [ObservableProperty] private string errorMessage = string.Empty;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        Password.Length >= 6 &&
        Email.Contains('@');

    private bool CanRegister() =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        Password.Length >= 6 &&
        Email.Contains('@');

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        IsLoading    = true;
        ErrorMessage = string.Empty;

        var (success, error) = await _authService.LoginAsync(Email, Password);

        if (success)
            await Shell.Current.GoToAsync("///ProductsPage");
        else
            ErrorMessage = error ?? "Error al iniciar sesión";

        IsLoading = false;
    }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync()
    {
        IsLoading    = true;
        ErrorMessage = string.Empty;

        var policyError = ValidatePasswordPolicy(Password);
        if (policyError is not null)
        {
            ErrorMessage = policyError;
            IsLoading    = false;
            return;
        }

        var (success, error) = await _authService.RegisterAsync(Name, Email, Password);

        if (success)
            await Shell.Current.GoToAsync("///ProductsPage");
        else
            ErrorMessage = error ?? "Error al registrarse";

        IsLoading = false;
    }

    [RelayCommand]
    private async Task GoToRegisterAsync() =>
        await Shell.Current.GoToAsync("RegisterPage");

    [RelayCommand]
    private async Task GoToLoginAsync() =>
        await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task ForgotPasswordAsync() =>
        await Shell.Current.DisplayAlert("THE DROP", "Te enviaremos un enlace a tu correo.", "OK");
    [RelayCommand]
    private async Task ContinueAsGuestAsync()
    {
        _authService.ContinueAsGuest();
        await Shell.Current.GoToAsync("///ProductsPage");
    }

    private static string? ValidatePasswordPolicy(string pwd)
    {
        if (pwd.Length is < 6 or > 12)
            return "La contraseña debe tener entre 6 y 12 caracteres.";
        if (!Regex.IsMatch(pwd, "[A-Z]"))
            return "Debe incluir al menos una letra mayúscula.";
        if (!Regex.IsMatch(pwd, "[a-z]"))
            return "Debe incluir al menos una letra minúscula.";
        if (!Regex.IsMatch(pwd, "[0-9]"))
            return "Debe incluir al menos un número.";
        if (!Regex.IsMatch(pwd, "[^A-Za-z0-9]"))
            return "Debe incluir al menos un signo (ej. !@#$%).";
        return null;
    }
}
