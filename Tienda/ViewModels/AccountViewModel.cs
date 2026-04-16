// Tienda/ViewModels/AccountViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class AccountViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty] private string userName = "NOMADE_USER";
    [ObservableProperty] private string userLevel = "ELITE MEMBER";
    [ObservableProperty] private string avatarInitials = "NU";
    [ObservableProperty] private int totalOrders = 12;
    [ObservableProperty] private decimal totalSpent = 1432.50m;
    [ObservableProperty] private int dropsParticipated = 3;

    public bool IsLoggedIn => _authService?.IsLoggedIn == true;

    public AccountViewModel(IAuthService authService)
    {
        _authService = authService;
        LoadUserData();
    }

    private void LoadUserData()
    {
        if (!IsLoggedIn) return;

        var email = _authService.GetUserEmail() ?? "NOMADE_USER";
        UserName = email.Split('@')[0].ToUpperInvariant();

        var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        AvatarInitials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}"
            : UserName.Length >= 2 ? UserName[..2] : UserName;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            bool answer = await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.DisplayAlert("LOGOUT", "¿Cerrar sesión?", "SÍ", "NO"));

            if (answer)
            {
                _authService.Logout();
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Shell.Current.GoToAsync("///LoginPage"));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Logout Error] {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GoToOrderHistoryAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.GoToAsync(nameof(Views.OrderHistoryPage)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GoToAddressesAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.GoToAsync(nameof(Views.AddressesPage)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GoToNotificationsAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
                Shell.Current.GoToAsync(nameof(Views.NotificationsPage)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}");
        }
    }
}