// Tienda/ViewModels/AccountViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tienda.Models;
using Tienda.Services;

namespace Tienda.ViewModels;

public partial class AccountViewModel : ObservableObject
{
    private readonly IAuthService    _authService;
    private readonly IClienteService _clienteService;
    private readonly IOrdenService   _ordenService;
    private readonly IPagosService   _pagosService;

    [ObservableProperty] private string  userName          = "NOMADE_USER";
    [ObservableProperty] private string  userLevel         = "ELITE MEMBER";
    [ObservableProperty] private string  avatarInitials    = "NU";
    [ObservableProperty] private int     totalOrders       = 0;
    [ObservableProperty] private decimal totalSpent        = 0m;
    [ObservableProperty] private int     dropsParticipated = 0;
    [ObservableProperty] private int     addressCount      = 0;

    // Crédito
    [ObservableProperty] private decimal limiteCredito;
    [ObservableProperty] private decimal creditoDisponible;
    [ObservableProperty] private bool    tienePlanes;

    [ObservableProperty]
    private ObservableCollection<Address> addresses = new();
    partial void OnAddressesChanged(ObservableCollection<Address> value) =>
        AddressCount = value.Count;

    [ObservableProperty]
    private ObservableCollection<PlanPagoInfo> planesActivos = new();

    public decimal CreditoUsado => LimiteCredito - CreditoDisponible;
    public double  CreditoPorcentaje => LimiteCredito > 0
        ? (double)(CreditoDisponible / LimiteCredito)
        : 0;

    public bool IsLoggedIn => _authService?.IsLoggedIn == true;

    public AccountViewModel(
        IAuthService    authService,
        IClienteService clienteService,
        IOrdenService   ordenService,
        IPagosService   pagosService)
    {
        _authService    = authService;
        _clienteService = clienteService;
        _ordenService   = ordenService;
        _pagosService   = pagosService;
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
    public async Task LoadStatsAsync()
    {
        if (!IsLoggedIn) return;
        var userId = _authService.GetUserId();

        var ordenesTask   = _ordenService.ObtenerMisOrdenesAsync();
        var dirsTask      = _clienteService.GetDireccionesAsync(userId);
        var creditoTask   = _clienteService.GetCreditoAsync(userId);
        var planesTask    = _pagosService.ObtenerPlanesAsync(userId);

        await Task.WhenAll(ordenesTask, dirsTask, creditoTask, planesTask);

        var ordenes = ordenesTask.Result;
        TotalOrders       = ordenes.Count;
        TotalSpent        = ordenes.Where(o => o.Estado == "confirmada").Sum(o => o.Total);
        DropsParticipated = ordenes.Select(o => o.CreadoEn.Date).Distinct().Count();

        Addresses = new ObservableCollection<Address>(dirsTask.Result);

        if (creditoTask.Result is { } credito)
        {
            LimiteCredito     = credito.LimiteCredito;
            CreditoDisponible = credito.CreditoDisponible;
            OnPropertyChanged(nameof(CreditoUsado));
            OnPropertyChanged(nameof(CreditoPorcentaje));
        }

        PlanesActivos = new ObservableCollection<PlanPagoInfo>(planesTask.Result);
        TienePlanes   = PlanesActivos.Count > 0;
    }

    [RelayCommand]
    public async Task LoadAddressesAsync()
    {
        if (!IsLoggedIn) return;
        var dirs = await _clienteService.GetDireccionesAsync(_authService.GetUserId());
        Addresses = new ObservableCollection<Address>(dirs);
    }

    [RelayCommand]
    private async Task AddAddressAsync()
    {
        await Shell.Current.DisplayAlert(
            "Nueva dirección", "Próximamente: formulario para agregar dirección.", "OK");
    }

    [RelayCommand]
    private async Task PagarCuotaAsync(PlanPagoInfo plan)
    {
        await Shell.Current.GoToAsync("CuotasPage",
            new Dictionary<string, object> { ["PlanId"] = plan.Id });
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
                UserName          = "NOMADE_USER";
                UserLevel         = "ELITE MEMBER";
                AvatarInitials    = "NU";
                TotalOrders       = 0;
                TotalSpent        = 0m;
                DropsParticipated = 0;
                LimiteCredito     = 0;
                CreditoDisponible = 0;
                Addresses         = new ObservableCollection<Address>();
                PlanesActivos     = new ObservableCollection<PlanPagoInfo>();
                TienePlanes       = false;
                OnPropertyChanged(nameof(IsLoggedIn));
                OnPropertyChanged(nameof(CreditoUsado));
                OnPropertyChanged(nameof(CreditoPorcentaje));

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
        try { await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync(nameof(Views.OrderHistoryPage))); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}"); }
    }

    [RelayCommand]
    private async Task GoToAddressesAsync()
    {
        try { await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync(nameof(Views.AddressesPage))); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}"); }
    }

    [RelayCommand]
    private async Task GoToNotificationsAsync()
    {
        try { await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync(nameof(Views.NotificationsPage))); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Nav Error] {ex.Message}"); }
    }
}
