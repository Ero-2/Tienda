// ViewModels/AccountViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tienda.Models;

namespace Tienda.ViewModels;

public partial class AccountViewModel : ObservableObject
{
    [ObservableProperty]
    private string userName = "NOMADE_USER";

    [ObservableProperty]
    private string userLevel = "ELITE MEMBER";

    [ObservableProperty]
    private string avatarUrl = "user_avatar.png";

    [RelayCommand]
    private async Task UpdateProfile()
    {
        // Para evitar errores de "no existe en el contexto", 
        // asignamos al campo privado y el toolkit notificará a la propiedad pública.
        var nuevoNombre = "NOMADE_" + new Random().Next(100, 999);
        userName = nuevoNombre;

        // Notificamos a la app
        WeakReferenceMessenger.Default.Send(new UserUpdatedMessage(nuevoNombre));

        await Shell.Current.DisplayAlert("SISTEMA", "DROP INFO: Perfil Actualizado", "OK");
    }

    [RelayCommand]
    private async Task Logout()
    {
        bool answer = await Shell.Current.DisplayAlert("LOGOUT", "¿Cerrar sesión?", "SÍ", "NO");
        if (answer) await Shell.Current.GoToAsync("///ProductsPage");
    }
}